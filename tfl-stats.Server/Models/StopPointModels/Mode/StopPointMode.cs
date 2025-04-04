using Newtonsoft.Json;

namespace tfl_stats.Server.Models.StopPointModels.Mode
{
    public class StopPointMode
    {
        //[JsonRequired]
        [JsonProperty("naptanId")]
        public string NaptanId { get; set; }

        //[JsonRequired]
        [JsonProperty("indicator")]
        public string Indicator { get; set; }

        //[JsonRequired]
        [JsonProperty("stopLetter")]
        public string StopLetter { get; set; }

        //[JsonRequired]
        [JsonProperty("modes")]
        public List<string> Modes { get; set; }

        //[JsonRequired]
        [JsonProperty("icsCode")]
        public string IcsCode { get; set; }

        //[JsonRequired]
        [JsonProperty("stopType")]
        public string StopType { get; set; }

        //[JsonProperty("stationNaptan")]
        public string StationNaptan { get; set; }

        //[JsonProperty("hubNaptanCode")]
        public string HubNaptanCode { get; set; }

        //[JsonProperty("lines")]
        public List<object> Lines { get; set; }

        //[JsonProperty("lineGroup")]
        public List<object> LineGroup { get; set; }

        //[JsonProperty("lineModeGroups")]
        public List<object> LineModeGroups { get; set; }

        //[JsonRequired]
        [JsonProperty("status")]
        public bool Status { get; set; }

        //[JsonRequired]
        [JsonProperty("id")]
        public string Id { get; set; }

        //[JsonRequired]
        [JsonProperty("commonName")]
        public string CommonName { get; set; }

        //[JsonRequired]
        [JsonProperty("placeType")]
        public string PlaceType { get; set; }

        //[JsonRequired]
        [JsonProperty("additionalProperties")]
        public List<AdditionalProperty> AdditionalProperties { get; set; }

        //[JsonRequired]
        [JsonProperty("children")]
        public List<object> Children { get; set; }

        //[JsonRequired]
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        //[JsonRequired]
        [JsonProperty("lon")]
        public double Longitude { get; set; }

    }
}
