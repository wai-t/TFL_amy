using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class StopPoint
    {
        [JsonProperty("naptanId")]
        //[JsonRequired]
        public string NaptanId { get; set; } = string.Empty;

        [JsonProperty("icsCode")]
        [JsonRequired]
        public string IcsCode { get; set; } = string.Empty;

        [JsonProperty("commonName")]
        [JsonRequired]
        public string CommonName { get; set; } = string.Empty;

        [JsonProperty("lat")]
        [JsonRequired]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        [JsonRequired]
        public double Longitude { get; set; }

    }
}
