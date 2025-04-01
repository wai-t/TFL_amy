using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class StopPointResponse
    {
        [JsonProperty("matches")]
        [JsonRequired]
        public List<MatchedStop> Matches { get; set; } = new List<MatchedStop>();
    }
}
