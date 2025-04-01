using System.Text.Json.Serialization;

namespace tfl_stats.Server.Models.LineModels
{
    public class Line
    {
        [JsonPropertyName("id")]
        [JsonRequired]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        [JsonRequired]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("modeName")]
        [JsonRequired]
        public string ModeName { get; set; } = string.Empty;

        [JsonPropertyName("disruptions")]
        [JsonRequired]
        public List<Disruption> Disruptions { get; set; } = new List<Disruption>();

        [JsonPropertyName("created")]
        [JsonRequired]
        public DateTime Created { get; set; }

        [JsonPropertyName("modified")]
        [JsonRequired]
        public DateTime Modified { get; set; }

        [JsonPropertyName("lineStatuses")]
        [JsonRequired]
        public List<LineStatus> LineStatuses { get; set; } = new List<LineStatus>();

        [JsonPropertyName("routeSections")]
        [JsonRequired]
        public List<RouteSection> RouteSections { get; set; } = new List<RouteSection>();

        [JsonPropertyName("serviceTypes")]
        [JsonRequired]
        public List<LineServiceTypeInfo> ServiceTypes { get; set; } = new List<LineServiceTypeInfo>();

        [JsonPropertyName("crowding")]
        [JsonRequired]
        public Crowding Crowding { get; set; } = new Crowding();
    }
}
