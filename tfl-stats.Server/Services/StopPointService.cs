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

        public async Task PreloadStopPoints()
        {
            await _preloadLock.WaitAsync();

            try
            {
                var existing = await _cache.GetAsync<List<StopPointSummary>>(CacheKeys.AllStopPoints);
                if (existing != null)
                {
                    _logger.LogInformation("Preload skipped – already cached.");
                    return;
                }

                var url = "StopPoint/Mode/tube";
                var response = await _apiClient.GetFromApi<StopPointModeResponse>(url);

                if (response?.StopPoints != null)
                {
                    var allStopPoints = response.StopPoints
                        .Where(sp => sp.StopType == "NaptanMetroStation")
                        .Select(sp => new StopPointSummary
                        {
                            NaptanId = sp.NaptanId,
                            CommonName = sp.CommonName
                        })
                        .Distinct()
                        .ToList();

                    await _cache.SetAsync(CacheKeys.AllStopPoints, allStopPoints, TimeSpan.FromDays(1));
                    _logger.LogInformation("Preloaded and cached all stop points.");
                }
                else
                {
                    _logger.LogWarning("No stop points found to preload.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during stop point preload.");
            }
            finally
            {
                _preloadLock.Release();
            }
        }

        public async Task<List<StopPointSummary>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<StopPointSummary>();

            var cacheKey = CacheKeys.Autocomplete(query);

            try
            {
                var cachedSuggestions = await _cache.GetAsync<List<StopPointSummary>>(cacheKey);
                if (cachedSuggestions != null)
                {
                    _logger.LogInformation("Cache hit for autocomplete '{Query}'", query);
                    return cachedSuggestions;
                }

                var allStopPoints = await _cache.GetAsync<List<StopPointSummary>>(CacheKeys.AllStopPoints);
                if (allStopPoints == null)
                {
                    _logger.LogInformation("Cache miss: preloading stop points.");
                    await PreloadStopPoints();
                    allStopPoints = await _cache.GetAsync<List<StopPointSummary>>(CacheKeys.AllStopPoints);
                }

                if (allStopPoints != null)
                {
                    var suggestions = allStopPoints
                        .Where(sp => sp.CommonName.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (suggestions.Any())
                    {
                        await _cache.SetAsync(cacheKey, suggestions, TimeSpan.FromDays(1));
                        _logger.LogInformation("Autocomplete suggestions cached for '{Query}'", query);
                        return suggestions;
                    }
                    else
                    {
                        _logger.LogInformation("No autocomplete suggestions found for '{Query}'", query);
                    }
                }

                _logger.LogWarning("Using fallback to fetch suggestions directly from API.");
                return await FetchFromApiAndCache(query, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing cache for '{Query}'", query);
                return new List<StopPointSummary>();
            }
        }

        private async Task<List<StopPointSummary>> FetchFromApiAndCache(string query, string cacheKey)
        {
            try
            {
                var url = "StopPoint/Mode/tube";
                var response = await _apiClient.GetFromApi<StopPointModeResponse>(url);

                if (response?.StopPoints == null)
                {
                    _logger.LogWarning("Fallback API call returned no data.");
                    return new List<StopPointSummary>();
                }

                var allStopPoints = response.StopPoints
                    .Where(sp => sp.StopType == "NaptanMetroStation")
                    .Select(sp => new StopPointSummary
                    {
                        NaptanId = sp.NaptanId,
                        CommonName = sp.CommonName
                    })
                    .Distinct()
                    .ToList();

                await _cache.SetAsync(CacheKeys.AllStopPoints, allStopPoints, TimeSpan.FromDays(1));

                var suggestions = allStopPoints
                    .Where(sp => sp.CommonName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (suggestions.Any())
                {
                    await _cache.SetAsync(cacheKey, suggestions, TimeSpan.FromDays(1));
                    _logger.LogInformation("Fallback API results cached for '{Query}'", query);
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback API call failed for '{Query}'", query);
                return new List<StopPointSummary>();
            }
        }
    }
}
