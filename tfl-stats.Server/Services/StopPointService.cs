using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models.StopPointModels;
using tfl_stats.Server.Models.StopPointModels.Mode;


namespace tfl_stats.Server.Services
{
    public class StopPointService
    {
        private readonly ApiClient _apiclient;
        private readonly IDatabase _cache;
        private readonly ILogger<StopPointService> _logger;
        private readonly string appId;
        private readonly string appKey;
        private readonly string baseUrl;

        public StopPointService(ApiClient apiClient,
            IOptions<AppSettings> options,
            IConnectionMultiplexer redis,
            ILogger<StopPointService> logger)
        {
            _apiclient = apiClient;
            _cache = redis.GetDatabase();
            _logger = logger;
            appId = options.Value.appId ?? throw new ArgumentNullException(nameof(appId));
            appKey = options.Value.appKey ?? throw new ArgumentNullException(nameof(appKey));
            baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<string?> GetStopPointId(string location)
        {
            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(location)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await _apiclient.GetFromApi<StopPointSearchResponse>(url, "GetStopPointId");

            var bestMatch = stopPointResponse?.Matches?.FirstOrDefault(sp => sp.Modes.Contains("tube"));
            return bestMatch?.IcsId;
        }

        public async Task PreloadStopPoints()
        {
            try
            {
                string url = $"{baseUrl}StopPoint/Mode/tube?app_id={appId}&app_key={appKey}";
                var stopPointResponse = await _apiclient.GetFromApi<StopPointModeResponse>(url, "PreloadStopPoints");


                if (stopPointResponse?.StopPoints != null)
                {
                    var allStopPoints = stopPointResponse.StopPoints.Select(sp => sp.CommonName).Distinct().ToList();

                    await _cache.StringSetAsync("allStopPoints", JsonConvert.SerializeObject(allStopPoints), TimeSpan.FromDays(1));
                    _logger.LogInformation("Preloaded and cached all stop points.");
                }
                else
                {
                    _logger.LogWarning("No stop points found to preload.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading stop points");
            }
        }

        public async Task<List<string>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var cacheKey = $"autocomplete:{query.ToLowerInvariant()}";

            try
            {
                var cachedAllStopPoints = await _cache.StringGetAsync("allStopPoints");
                if (cachedAllStopPoints.HasValue)
                {
                    _logger.LogInformation("CACHE HIT for all stop points.");
                    var allStopPoints = JsonConvert.DeserializeObject<List<string>>(cachedAllStopPoints.ToString());

                    if (allStopPoints != null)
                    {
                        var suggestions = allStopPoints.Where(sp => sp.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (suggestions.Any())
                        {
                            return suggestions;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Redis cache for all stop points");
            }

            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(query)}?modes=tube&app_id={appId}&app_key={appKey}";
            var stopPointResponse = await _apiclient.GetFromApi<StopPointSearchResponse>(url, "GetAutocompleteSuggestions");

            var apiSuggestions = stopPointResponse?.Matches?.Select(sp => sp.Name).ToList() ?? new List<string>();

            if (apiSuggestions.Any())
            {
                try
                {
                    await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(apiSuggestions), TimeSpan.FromHours(1));
                    _logger.LogInformation("CACHE MISS => Fetched and cached autocomplete suggestions for '{Query}'", query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to Redis cache for key '{CacheKey}'", cacheKey);
                }
            }

            return apiSuggestions;
        }
    }
}
