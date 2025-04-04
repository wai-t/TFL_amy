using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class StopPointModeResponse
    {
        [JsonRequired]
        [JsonProperty("stopPoints")]
        public List<StopPointMode> StopPoints { get; set; }
    }
}
