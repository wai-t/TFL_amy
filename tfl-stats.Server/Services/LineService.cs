using tfl_stats.Server.Client;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.LineModels;

#if USE_TFL_MODEL
using Line = tfl_stats.Tfl.Line;
#endif

namespace tfl_stats.Server.Services
{
    public class LineService
    {
        private readonly ApiClient _apiClient;

        private ILogger<LineService> _logger;

        public LineService(ApiClient apiClient, ILogger<LineService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<ResponseResult<List<Line>>> GetLine()
        {

            string url = "Line/Mode/tube/Status";

            var lineResponse = await _apiClient.GetFromApi<List<Line>>(url);

            if (lineResponse != null)
            {
                return new ResponseResult<List<Line>>(true, lineResponse, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Line>>(false, new List<Line>(), ResponseStatus.NotFound);
        }
    }
}
