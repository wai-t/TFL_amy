using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class StopPointMode
    {
        //[JsonRequired]
        [JsonProperty("naptanId")]
        public string NaptanId { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("indicator")]
        public string Indicator { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("stopLetter")]
        public string StopLetter { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("modes")]
        public List<string> Modes { get; set; } = new List<string>();

        //[JsonRequired]
        //[JsonProperty("icsCode")]
        public string IcsCode { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("stopType")]
        public string StopType { get; set; } = string.Empty;

        [JsonProperty("stationNaptan")]
        public string StationNaptan { get; set; } = string.Empty;

        [JsonProperty("hubNaptanCode")]
        public string HubNaptanCode { get; set; } = string.Empty;

        [JsonProperty("lines")]
        public List<object> Lines { get; set; } = new List<object>();

        [JsonProperty("lineGroup")]
        public List<object> LineGroup { get; set; } = new List<object>();

        [JsonProperty("lineModeGroups")]
        public List<object> LineModeGroups { get; set; } = new List<object>();

        //[JsonRequired]
        [JsonProperty("status")]
        public bool Status { get; set; }

        //[JsonRequired]
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("commonName")]
        public string CommonName { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("placeType")]
        public string PlaceType { get; set; } = string.Empty;

        //[JsonRequired]
        [JsonProperty("additionalProperties")]
        public List<AdditionalProperty> AdditionalProperties { get; set; } = new List<AdditionalProperty>();

        //[JsonRequired]
        [JsonProperty("children")]
        public List<object> Children { get; set; } = new List<object>();

        //[JsonRequired]
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        //[JsonRequired]
        [JsonProperty("lon")]
        public double Longitude { get; set; }
    }
}
