using Newtonsoft.Json;

namespace tfl_stats.Server.Models.LineModels
{
    public class LineServiceTypeInfo
    {
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("uri")]
        [JsonRequired]
        public string Uri { get; set; } = string.Empty;
    }
}
