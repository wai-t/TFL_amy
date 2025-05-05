using Microsoft.Extensions.Options;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
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
            IOptions<AppSettings> options,
            ICacheService cache,
            ILogger<StopPointService> logger)
        {
            _apiClient = apiClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string?> GetStopPointId(string location)
        {
            var url = $"StopPoint/Search/{Uri.EscapeDataString(location)}";
            var response = await _apiClient.GetFromApi<StopPointSearchResponse>(url);

            return response?.Matches?.FirstOrDefault(sp => sp.Modes.Contains("tube"))?.IcsId;
        }

        public async Task PreloadStopPoints()
        {
            await _preloadLock.WaitAsync();

            try
            {
                var existing = await _cache.GetAsync<List<string>>(CacheKeys.AllStopPoints);
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
                        .Select(sp => sp.CommonName)
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

        public async Task<List<string>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var cacheKey = CacheKeys.Autocomplete(query);

            try
            {
                var cachedSuggestions = await _cache.GetAsync<List<string>>(cacheKey);
                if (cachedSuggestions != null)
                {
                    _logger.LogInformation("CACHE HIT for autocomplete '{Query}'", query);
                    return cachedSuggestions;
                }

                var allStopPoints = await _cache.GetAsync<List<string>>(CacheKeys.AllStopPoints);
                if (allStopPoints != null)
                {
                    _logger.LogInformation("CACHE HIT for all stop points.");

                    var suggestions = allStopPoints
                        .Where(sp => sp.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (suggestions.Any())
                    {
                        await _cache.SetAsync(cacheKey, suggestions, TimeSpan.FromHours(1));
                        return suggestions;
                    }
                }
                else
                {
                    _logger.LogInformation("CACHE MISS: Preloading stop points.");
                    await PreloadStopPoints();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing cache for '{Query}'", query);
            }

            return await FetchFromApiAndCache(query, cacheKey);
        }

        private async Task<List<string>> FetchFromApiAndCache(string query, string cacheKey)
        {
            var url = $"StopPoint/Search/{Uri.EscapeDataString(query)}?modes=tube";
            var response = await _apiClient.GetFromApi<StopPointSearchResponse>(url);

            var apiSuggestions = response?.Matches?.Select(sp => sp.Name).ToList() ?? new List<string>();

            if (apiSuggestions.Any())
            {
                try
                {
                    await _cache.SetAsync(cacheKey, apiSuggestions, TimeSpan.FromDays(1));
                    _logger.LogInformation("CACHE MISS => Fetched and cached autocomplete for '{Query}'", query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to cache for '{CacheKey}'", cacheKey);
                }
            }

            return apiSuggestions;
        }
    }
}
