using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels
{
    public class StopPointResponse
    {
        [JsonRequired]
        [JsonProperty("naptanId")]
        public string NaptanId { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty("commonName")]
        public string CommonName { get; set; } = string.Empty;

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("modes")]
        public List<string> Modes { get; set; } = new List<string>();

        [JsonProperty("icsCode")]
        public string IcsCode { get; set; } = string.Empty;

        [JsonProperty("stopType")]
        public string StopType { get; set; } = string.Empty;

        [JsonProperty("stationNaptan")]
        public string StationNaptan { get; set; } = string.Empty;

        [JsonProperty("lines")]
        public List<Line> Lines { get; set; } = new List<Line>();

        [JsonProperty("lineGroup")]
        public List<LineGroup> LineGroup { get; set; } = new List<LineGroup>();

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("placeType")]
        public string PlaceType { get; set; } = string.Empty;

        [JsonProperty("additionalProperties")]
        public List<AdditionalProperty> AdditionalProperties { get; set; } = new List<AdditionalProperty>();
    }

    public class Line
    {
        [JsonRequired]
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("uri")]
        public string Uri { get; set; } = string.Empty;
    }

    public class LineGroup
    {
        [JsonRequired]
        [JsonProperty("stationAtcoCode")]
        public string StationAtcoCode { get; set; } = string.Empty;

        [JsonProperty("lineIdentifier")]
        public List<string> LineIdentifier { get; set; } = new List<string>();
    }

    public class AdditionalProperty
    {
        [JsonRequired]
        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("sourceSystemKey")]
        public string SourceSystemKey { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }
}
