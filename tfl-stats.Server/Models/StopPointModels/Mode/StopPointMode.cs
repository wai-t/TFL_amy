using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class StopPointMode
    {
        [JsonProperty("naptanId")]
        [JsonRequired]
        public string NaptanId { get; set; } = string.Empty;

        [JsonProperty("indicator")]
        public string Indicator { get; set; } = string.Empty;

        [JsonProperty("stopLetter")]
        public string StopLetter { get; set; } = string.Empty;

        [JsonProperty("modes")]
        [JsonRequired]
        public List<string> Modes { get; set; } = new List<string>();

        [JsonProperty("icsCode")]
        public string IcsCode { get; set; } = string.Empty;

        [JsonProperty("stopType")]
        public string StopType { get; set; } = string.Empty;

        [JsonProperty("stationNaptan")]
        public string StationNaptan { get; set; } = string.Empty;

        [JsonProperty("hubNaptanCode")]
        public string HubNaptanCode { get; set; } = string.Empty;

        [JsonProperty("lines")]
        public List<Line> Lines { get; set; } = new List<Line>();

        [JsonProperty("lineGroup")]
        public List<LineGroup> LineGroup { get; set; } = new List<LineGroup>();

        [JsonProperty("lineModeGroups")]
        public List<LineModeGroups> LineModeGroups { get; set; } = new List<LineModeGroups>();

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("commonName")]
        [JsonRequired]
        public string CommonName { get; set; } = string.Empty;

        [JsonProperty("placeType")]
        public string PlaceType { get; set; } = string.Empty;

        [JsonProperty("additionalProperties")]
        public List<AdditionalProperty> AdditionalProperties { get; set; } = new List<AdditionalProperty>();

        [JsonProperty("children")]
        public List<Children> Children { get; set; } = new List<Children>();

        [JsonProperty("lat")]
        [JsonRequired]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        [JsonRequired]
        public double Longitude { get; set; }
    }

    public class Children
    {
    }

    public class LineModeGroups
    {
    }
}
