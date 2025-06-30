using tfl_stats.Server.Client;
using tfl_stats.Server.Models;

#if USE_TFL_MODEL
    using Journey = tfl_stats.Tfl.Journey2;
    using JourneyRequest = tfl_stats.Server.Models.JourneyModels.JourneyRequest;
    using JourneyResponse = tfl_stats.Server.Models.JourneyModels.JourneyResponse;
#else
    using tfl_stats.Server.Models.JourneyModels;
#endif

namespace tfl_stats.Server.Services
{
    public class JourneyService
    {
        private readonly ApiClient _apiclient;
        private readonly ILogger<JourneyService> _logger;
        private readonly StopPointService _stopPointService;

        public JourneyService(ApiClient apiClient,
            StopPointService stopPointService,
            ILogger<JourneyService> logger)
        {
            _apiclient = apiClient;
            _stopPointService = stopPointService;
            _logger = logger;
        }

        public async Task<ResponseResult<List<Journey>>> GetJourney(JourneyRequest journeyRequest)
        {


            if (string.IsNullOrEmpty(journeyRequest.FromNaptanId) || string.IsNullOrEmpty(journeyRequest.ToNaptanId))
            {
                return new ResponseResult<List<Journey>>(false, new List<Journey>(), ResponseStatus.BadRequest);
            }

            string url = $"Journey/journeyresults/{Uri.EscapeDataString(journeyRequest.FromNaptanId)}/to/{Uri.EscapeDataString(journeyRequest.ToNaptanId)}";

            var journeyResponse = await _apiclient.GetFromApi<JourneyResponse>(url);

            if (journeyResponse?.Journeys != null)
            {
                return new ResponseResult<List<Journey>>(true, journeyResponse.Journeys, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Journey>>(false, new List<Journey>(), ResponseStatus.NotFound);
        }
    }

}
