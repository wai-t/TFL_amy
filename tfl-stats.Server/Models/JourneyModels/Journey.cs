using Newtonsoft.Json;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class Journey
    {
        [JsonProperty("startDateTime")]
        [JsonRequired]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("arrivalDateTime")]
        [JsonRequired]
        public DateTime ArrivalDateTime { get; set; }

        [JsonProperty("duration")]
        [JsonRequired]
        public int Duration { get; set; }

        [JsonProperty("alternativeRoute")]
        [JsonRequired]
        public bool AlternativeRoute { get; set; }

        [JsonProperty("legs")]
        [JsonRequired]
        public List<JourneyLeg> Legs { get; set; } = new List<JourneyLeg>();
    }
}
