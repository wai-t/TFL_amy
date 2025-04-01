using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Path
    {
        [JsonProperty("lineString")]
        [JsonRequired]
        public string LineString { get; set; } = string.Empty;

        [JsonProperty("stopPoints")]
        [JsonRequired]
        public List<Point> StopPoints { get; set; } = new List<Point>();

    }
}
