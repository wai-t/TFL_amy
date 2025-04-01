using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Obstacle
    {
        [JsonProperty("type")]
        [JsonRequired]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("incline")]
        [JsonRequired]
        public string Incline { get; set; } = string.Empty;

        [JsonProperty("stopId")]
        [JsonRequired]
        public long StopId { get; set; }

        [JsonProperty("position")]
        [JsonRequired]
        public string Position { get; set; } = string.Empty;
    }
}
