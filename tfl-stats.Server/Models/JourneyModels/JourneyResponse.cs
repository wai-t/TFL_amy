using Newtonsoft.Json;


namespace tfl_stats.Server.Models.JourneyModels
{
#if USE_TFL_MODEL
    using JourneyT = tfl_stats.Tfl.Journey2;
#else
    using JourneyT = Journey;
#endif

    public class JourneyResponse
    {
        [JsonProperty("journeys")]
        [JsonRequired]
        public List<JourneyT> Journeys { get; set; } = new List<JourneyT>();
    }
}
