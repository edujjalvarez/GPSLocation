using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace GPSLocation.Model
{
    public class Location
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        [Version]
        public string Version { get; set; }
    }
}
