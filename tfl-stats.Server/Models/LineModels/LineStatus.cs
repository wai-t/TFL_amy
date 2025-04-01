using Newtonsoft.Json;

namespace tfl_stats.Server.Models.LineModels
{
    public class LineStatus
    {
        [JsonProperty("id")]
        [JsonRequired]
        public int Id { get; set; }

        [JsonProperty("statusSeverity")]
        [JsonRequired]
        public int StatusSeverity { get; set; }

        [JsonProperty("statusSeverityDescription")]
        [JsonRequired]
        public string StatusSeverityDescription { get; set; } = string.Empty;

        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonProperty("created")]
        [JsonRequired]
        public DateTime Created { get; set; }

        [JsonProperty("validityPeriods")]
        public List<ValidityPeriod> ValidityPeriods { get; set; } = new List<ValidityPeriod>();

        [JsonProperty("disruption")]
        public Disruption Disruption { get; set; } = new Disruption();
    }
}
