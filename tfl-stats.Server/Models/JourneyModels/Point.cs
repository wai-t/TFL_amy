using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Point
    {
        [JsonProperty("id")]
        //[JsonRequired]
        public string NaptanId { get; set; } = string.Empty;

        [JsonProperty("name")]
        [JsonRequired]
        public string CommonName { get; set; } = string.Empty;
    }
}
