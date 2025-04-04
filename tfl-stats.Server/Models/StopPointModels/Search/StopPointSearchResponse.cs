using Newtonsoft.Json;
using tfl_stats.Server.Models.JourneyModels;

namespace tfl_stats.Server.Models.StopPointModels
{
    public class StopPointSearchResponse
    {
        [JsonProperty("matches")]
        [JsonRequired]
        public List<MatchedStop> Matches { get; set; } = new List<MatchedStop>();
    }
}
