using tfl_stats.Server.Client;
using tfl_stats.Server.Models.StopPointModels;
using tfl_stats.Server.Models.StopPointModels.Mode;
using tfl_stats.Server.Services.Cache;

namespace tfl_stats.Server.Services
{
    public class StopPointService
    {
        private readonly ApiClient _apiClient;
        private readonly ICacheService _cache;
        private readonly ILogger<StopPointService> _logger;

        private static readonly SemaphoreSlim _preloadLock = new(1, 1);

        public StopPointService(
            ApiClient apiClient,
            ICacheService cache,
            ILogger<StopPointService> logger)
        {
            _apiClient = apiClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<StopPointSummary>> PreloadStopPoints()
        {
            await _preloadLock.WaitAsync();

            try
            {
                var cached = await _cache.GetAsync<List<StopPointSummary>>(CacheKeys.AllStopPoints, FetchAllStopPoints);
                return cached!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading stop points.");
                return new List<StopPointSummary>();
            }
            finally
            {
                _preloadLock.Release();
            }
        }

        public async Task<List<StopPointSummary>> FetchAllStopPoints(string _)
        {
            var url = "StopPoint/Mode/tube";
            var response = await _apiClient.GetFromApi<StopPointModeResponse>(url);

            if (response?.StopPoints == null)
            {
                _logger.LogWarning("API returned no stop points.");
                return new List<StopPointSummary>();
            }

            var stopPoints = response.StopPoints
                .Where(sp => sp.StopType == "NaptanMetroStation")
                .Select(sp => new StopPointSummary
                {
                    NaptanId = sp.NaptanId,
                    CommonName = sp.CommonName
                })
                .ToList();

            await _cache.SetAsync(CacheKeys.AllStopPoints, stopPoints, TimeSpan.FromDays(1));
            _logger.LogInformation("Stop points fetched from API and cached.");

            return stopPoints;
        }
        public async Task<List<StopPointSummary>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<StopPointSummary>();

            try
            {
                var cachedSuggestions = await _cache.GetAsync<List<StopPointSummary>>(query, GetFilteredStopPoints);
                return cachedSuggestions!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing autocomplete for '{Query}'", query);
                return new List<StopPointSummary>();
            }
        }

        public async Task<List<StopPointSummary>> GetFilteredStopPoints(string query)
        {
            var allStopPoints = await PreloadStopPoints();

            var suggestions = allStopPoints
                .Where(sp => sp.CommonName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            if (suggestions.Any())
            {
                var cacheKey = CacheKeys.Autocomplete(query);

                await _cache.SetAsync(cacheKey, suggestions, TimeSpan.FromDays(1));
                _logger.LogInformation("Autocomplete suggestions cached for '{Query}'", query);
            }
            else
            {
                _logger.LogInformation("No autocomplete suggestions found for '{Query}'", query);
            }

            return suggestions;
        }
        internal async Task<List<StopPointSummary>> GetStopPointList()
        {
            return await PreloadStopPoints();
        }
    }
}