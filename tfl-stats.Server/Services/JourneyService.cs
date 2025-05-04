using Microsoft.Extensions.Options;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.JourneyModels;


namespace tfl_stats.Server.Services
{
    public class JourneyService
    {
        private readonly ApiClient _apiclient;
        private readonly ILogger<JourneyService> _logger;
        private readonly StopPointService _stopPointService;
        //private readonly string baseUrl;

        public JourneyService(ApiClient apiClient,
            IOptions<AppSettings> options,
            StopPointService stopPointService,
            ILogger<JourneyService> logger)
        {
            _apiclient = apiClient;
            _stopPointService = stopPointService;
            _logger = logger;
            //baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ResponseResult<List<Journey>>> GetJourney(JourneyRequest journeyRequest)
        {
            // Suggestion: Consider receiving the actual id of the StopPoint instead of using the name
            // again to do another query. Investigate which id is the right one to use; it looks like there
            // is a number of options including IcsId, NaptanId, etc. This would involve some changes on
            // the client side to store a list of (name, id) pairs for the StopPoints.
            var from = await _stopPointService.GetStopPointId(journeyRequest.From);
            var to = await _stopPointService.GetStopPointId(journeyRequest.To);

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return new ResponseResult<List<Journey>>(false, new List<Journey>(), ResponseStatus.BadRequest);
            }

            //string url = $"{baseUrl}Journey/journeyresults/{Uri.EscapeDataString(from)}/to/{Uri.EscapeDataString(to)}";
            string url = $"Journey/journeyresults/{Uri.EscapeDataString(from)}/to/{Uri.EscapeDataString(to)}";

            var journeyResponse = await _apiclient.GetFromApi<JourneyResponse>(url/*, "GetJourney"*/);

            if (journeyResponse?.Journeys != null)
            {
                return new ResponseResult<List<Journey>>(true, journeyResponse.Journeys, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Journey>>(false, new List<Journey>(), ResponseStatus.NotFound);
        }
    }

}
