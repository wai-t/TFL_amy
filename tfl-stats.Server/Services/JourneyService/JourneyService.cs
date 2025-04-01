using Microsoft.Extensions.Options;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.JourneyModels;


namespace tfl_stats.Server.Services.JourneyService
{
    public class JourneyService
    {
        //private readonly HttpClient _httpclient;
        private readonly ApiClient _apiclient;

        private ILogger<JourneyService> _logger;

        private readonly string appId;
        private readonly string appKey;
        private readonly string baseUrl;

        public JourneyService(ApiClient apiClient,
            IOptions<AppSettings> options, ILogger<JourneyService> logger)

        {
            //_httpclient = httpClient;
            _apiclient = apiClient;
            _logger = logger;
            appId = options.Value.appId ?? throw new ArgumentNullException(nameof(appId));
            appKey = options.Value.appKey ?? throw new ArgumentNullException(nameof(appKey));
            baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ResponseResult<List<Journey>>> GetJourney(JourneyRequest journeyRequest)
        {
            var from = await GetStopPointId(journeyRequest.From);
            var to = await GetStopPointId(journeyRequest.To);

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return new ResponseResult<List<Journey>>(false, [], ResponseStatus.BadRequest);
            }

            string url = $"{baseUrl}Journey/journeyresults/{Uri.EscapeDataString(from)}/to/{Uri.EscapeDataString(to)}?app_id={appId}&app_key={appKey}";
            var journeyResponse = await _apiclient.GetFromApi<JourneyResponse>(url, "getJourney");

            if (journeyResponse?.Journeys != null)
            {
                return new ResponseResult<List<Journey>>(true, journeyResponse.Journeys, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Journey>>(false, [], ResponseStatus.NotFound);
        }

        private async Task<string?> GetStopPointId(string location)
        {
            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(location)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await _apiclient.GetFromApi<StopPointResponse>(url, "GetStopPointId");

            var bestMatch = stopPointResponse?.Matches?.FirstOrDefault(sp => sp.Modes.Contains("tube"));
            return bestMatch?.IcsId;
        }

        public async Task<List<string>> GetAutocompleteSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            string url = $"{baseUrl}StopPoint/Search/{Uri.EscapeDataString(query)}?app_id={appId}&app_key={appKey}";
            var stopPointResponse = await _apiclient.GetFromApi<StopPointResponse>(url, "GetAutocompleteSuggestions");

            return stopPointResponse?.Matches?.Select(sp => sp.Name).ToList() ?? [];
        }
    }
}
