using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.JourneyModels;


namespace tfl_stats.Server.Services.JourneyService
{
    public class JourneyService
    {
        private readonly HttpClient _httpclient;
        private ILogger<JourneyService> _logger;

        private readonly string appId;
        private readonly string appKey;
        private readonly string baseUrl;

        public JourneyService(HttpClient httpClient,
            IOptions<AppSettings> options, ILogger<JourneyService> logger)
        {
            _httpclient = httpClient;
            _logger = logger;
            appId = options.Value.appId ?? throw new ArgumentNullException(nameof(appId));
            appKey = options.Value.appKey ?? throw new ArgumentNullException(nameof(appKey));
            baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ResponseResult<List<Journey>>> getJourney(JourneyRequest journeyRequest)
        {
            var from = await GetStopPointId(journeyRequest.From);
            var to = await GetStopPointId(journeyRequest.To);

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return new ResponseResult<List<Journey>>(false, [], ResponseStatus.BadRequest, "Invalid stop points.");
            }

            string url = $"{baseUrl}Journey/journeyresults/{Uri.EscapeDataString(from)}/to/{Uri.EscapeDataString(to)}?app_id={appId}&app_key={appKey}";
            var journeyResponse = await GetFromApi<JourneyResponse>(url, "getJourney");

            if (journeyResponse?.Journeys != null)
            {
                return new ResponseResult<List<Journey>>(true, journeyResponse.Journeys, ResponseStatus.Ok, "Request Succeeded");
            }

            return new ResponseResult<List<Journey>>(false, [], ResponseStatus.NotFound, "No journeys found in response.");

        }

        private async Task<string?> GetStopPointId(string location)
        {
            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(location)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await GetFromApi<StopPointResponse>(url, "GetStopPointId");

            var bestMatch = stopPointResponse?.Matches?.FirstOrDefault(sp => sp.Modes.Contains("tube"));
            return bestMatch?.IcsId;
        }

        public async Task<List<string>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(query)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await GetFromApi<StopPointResponse>(url, "GetAutocompleteSuggestions");

            return stopPointResponse?.Matches?.Select(sp => sp.Name).ToList() ?? [];
        }

        private async Task<T?> GetFromApi<T>(string url, string context)
        {
            try
            {
                var responseContent = await _httpclient.GetStringAsync(url);
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
