using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class AdditionalProperty
    {
        [JsonRequired]
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonRequired]
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("sourceSystemKey")]
        public string SourceSystemKey { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
