using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class JourneyResponse
    {
        [JsonProperty("journeys")]
        [JsonRequired]
        public List<Journey> Journeys { get; set; } = new List<Journey>();
    }
}
