using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace tfl_stats.Server.Client
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

        public async virtual Task<T?> GetFromApi<T>(string url, [CallerMemberName]string context="")
        {
            // Don't just log and "swallow" exceptions. Either handle them or
            // let them bubble up, even if it means the caller has to deal with them.
            try
            {
                var responseContent = await _httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{context} - Network error: {ex.Message}");
                throw;
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError($"{context} - Deserialization failed: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{context} - Unexpected error: {ex.Message}");
                throw;
            }

            //return default;
        }
    }
}
