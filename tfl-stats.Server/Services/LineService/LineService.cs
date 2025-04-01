using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using tfl_stats.Server.Configurations;
using tfl_stats.Server.Models.LineModels;

namespace tfl_stats.Server.Services.LineService
{
    public class LineService
    {
        private readonly HttpClient _httpClient;
        private readonly string appId;
        private readonly string appKey;
        private readonly string baseUrl;

        public LineService(HttpClient httpClient, IOptions<AppSettings> options)
        {
            _httpClient = httpClient;
            appId = options.Value.appId ?? throw new ArgumentNullException(nameof(appId));
            appKey = options.Value.appKey ?? throw new ArgumentNullException(nameof(appKey));
            baseUrl = options.Value.baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<List<Line>> getLine()
        {

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("TflApi configuration is missing.");
            }

            string url = $"{baseUrl}Line/Mode/tube/Status?app_id={appId}&app_key={appKey}";

            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<Line>>(response) ?? new List<Line>();
        }
    }
}
