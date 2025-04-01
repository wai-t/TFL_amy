using Microsoft.Extensions.Options;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models;
using tfl_stats.Server.Models.LineModels;

namespace tfl_stats.Server.Services.LineService
{
    public class LineService
    {
        //private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;

        private ILogger<LineService> _logger;

        private readonly string appId;
        private readonly string appKey;
        private readonly string baseUrl;

        public LineService(ApiClient apiClient, IOptions<AppSettings> options, ILogger<LineService> logger)
        {
            //_httpClient = httpClient;
            this._apiClient = apiClient;
            _logger = logger;
            appId = options.Value.appId ?? throw new ArgumentNullException(nameof(appId));
            appKey = options.Value.appKey ?? throw new ArgumentNullException(nameof(appKey));
            baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<ResponseResult<List<Line>>> GetLine()
        {

            string url = $"{baseUrl}Line/Mode/tube/Status?app_id={appId}&app_key={appKey}";

            var lineResponse = await _apiClient.GetFromApi<List<Line>>(url, "GetLine");

            if (lineResponse != null)
            {
                return new ResponseResult<List<Line>>(true, lineResponse, ResponseStatus.Ok);
            }

            return new ResponseResult<List<Line>>(false, [], ResponseStatus.NotFound);
        }
    }
}
