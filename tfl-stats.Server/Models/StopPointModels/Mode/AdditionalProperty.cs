using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class AdditionalProperty
    {
        [JsonProperty("category")]
        [JsonRequired]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("key")]
        [JsonRequired]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("sourceSystemKey")]
        public string SourceSystemKey { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }
}
