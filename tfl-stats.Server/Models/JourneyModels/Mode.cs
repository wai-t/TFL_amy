using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Mode
    {
        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; } = string.Empty;

    }
}
