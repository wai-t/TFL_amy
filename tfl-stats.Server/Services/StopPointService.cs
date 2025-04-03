using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models.JourneyModels;

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
            var stopPointResponse = await _apiclient.GetFromApi<StopPointResponse>(url, "GetStopPointId");

            var bestMatch = stopPointResponse?.Matches?.FirstOrDefault(sp => sp.Modes.Contains("tube"));
            return bestMatch?.IcsId;
        }

        public async Task<List<string>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var cacheKey = $"autocomplete:{query.ToLowerInvariant()}";

            try
            {
                var cached = await _cache.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
                    _logger.LogInformation("CACHE HIT Autocomplete for '{Query}'", query);
                    return JsonConvert.DeserializeObject<List<string>>(cached.ToString()) ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Redis cache for key '{CacheKey}'", cacheKey);
            }

            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(query)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await _apiclient.GetFromApi<StopPointResponse>(url, "GetAutocompleteSuggestions");

            var suggestions = stopPointResponse?.Matches?.Select(sp => sp.Name).ToList() ?? new List<string>();

            if (suggestions.Any())
            {
                try
                {
                    await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(suggestions), TimeSpan.FromHours(1));
                    _logger.LogInformation("CACHE MISS => Fetched and cached autocomplete suggestions for '{Query}'", query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to Redis cache for key '{CacheKey}'", cacheKey);
                }
            }

            return suggestions;
        }

    }
}
