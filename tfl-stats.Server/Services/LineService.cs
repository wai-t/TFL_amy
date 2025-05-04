using Microsoft.Extensions.Options;
using tfl_stats.Server.Client;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.LineModels;

namespace tfl_stats.Server.Services
{
    public class LineService
    {
        private readonly ApiClient _apiClient;

        private ILogger<LineService> _logger;
        //private readonly string baseUrl;

        public LineService(ApiClient apiClient, IOptions<AppSettings> options, ILogger<LineService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
            //baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ResponseResult<List<Line>>> GetLine()
        {

            //string url = $"{baseUrl}Line/Mode/tube/Status";
            string url = "Line/Mode/tube/Status";

            var lineResponse = await _apiClient.GetFromApi<List<Line>>(url/*, "GetLine"*/);

            if (lineResponse != null)
            {
                return new ResponseResult<List<Line>>(true, lineResponse, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Line>>(false, new List<Line>(), ResponseStatus.NotFound);
        }
    }
}
