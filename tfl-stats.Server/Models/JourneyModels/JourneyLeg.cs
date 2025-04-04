using Newtonsoft.Json;
using tfl_stats.Server.Models.StopPointModels;

namespace tfl_stats.Server.Models.JourneyModels
{
    public class JourneyLeg
    {
        [JsonProperty("departureTime")]
        [JsonRequired]
        public DateTime DepartureTime { get; set; } = DateTime.MinValue;

        [JsonProperty("arrivalTime")]
        [JsonRequired]
        public DateTime ArrivalTime { get; set; } = DateTime.MinValue;

        [JsonProperty("duration")]
        [JsonRequired]
        public int Duration { get; set; }

        [JsonProperty("departurePoint")]
        [JsonRequired]
        public StopPoint DeparturePoint { get; set; } = new StopPoint();

        [JsonProperty("arrivalPoint")]
        [JsonRequired]
        public StopPoint ArrivalPoint { get; set; } = new StopPoint();

        [JsonProperty("instruction")]
        [JsonRequired]
        public Instruction Instruction { get; set; } = new Instruction();

        [JsonProperty("path")]
        [JsonRequired]
        public Path Path { get; set; } = new Path();

        [JsonProperty("mode")]
        [JsonRequired]
        public Mode Mode { get; set; } = new Mode();

        [JsonProperty("obstacles")]
        [JsonRequired]
        public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
    }
}
