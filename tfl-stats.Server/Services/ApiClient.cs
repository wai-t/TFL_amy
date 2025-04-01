using Newtonsoft.Json;

namespace tfl_stats.Server.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T?> GetFromApi<T>(string url, string context)
        {
            try
            {
                var responseContent = await _httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{context} - Network error: {ex.Message}");
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError($"{context} - Deserialization failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{context} - Unexpected error: {ex.Message}");
            }

            return default;
        }
    }
}
