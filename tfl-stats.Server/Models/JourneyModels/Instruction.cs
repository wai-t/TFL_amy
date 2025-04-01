using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Instruction
    {
        [JsonProperty("summary")]
        [JsonRequired]
        public string? Summary { get; set; }

        [JsonProperty("detailed")]
        [JsonRequired]
        public string? Detailed { get; set; }
    }
}
