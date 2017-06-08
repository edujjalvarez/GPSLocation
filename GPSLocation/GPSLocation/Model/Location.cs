using Microsoft.WindowsAzure.MobileServices;

namespace GPSLocation.Model
{
    public class Location
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Image { get; set; }

        [Version]
        public string AzureVersion { get; set; }
    }
}
