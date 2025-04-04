using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class MatchedStop
    {
        [JsonProperty("icsId")]
        [JsonRequired]
        public string IcsId { get; set; } = string.Empty;

        [JsonProperty("modes")]
        [JsonRequired]
        public string[] Modes { get; set; } = Array.Empty<string>();

        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; } = string.Empty;
    }
}