using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.Model.Entities;
using GPSLocation.Utils;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace GPSLocation.ViewModel
{
    public class LocationVM : ObservableBaseObject
    {
        private static string _tag = "LocationVM";

        private double lngX;
        public double LngX
        {
            get { return lngX; }
            set { lngX = value; OnPropertyChanged(); }
        }

        private double latY;
        public double LatY
        {
            get { return latY; }
            set { latY = value; OnPropertyChanged(); }
        }

        private double accuracy;
        public double Accuracy
        {
            get { return accuracy; }
            set { accuracy = value; OnPropertyChanged(); }
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged(); }
        }
        public Command StartSearchGPSCommand { get; set; }

        public LocationVM()
        {
            IsBusy = false;
            StartSearchGPSCommand = new Command((obj) => GetGPS());
        }
        private async void GetGPS()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                int precision = Settings.DesiredAccuracy;
                Log.GetInstance().Print(_tag, "GetGPS() >>> Precisión deseada = {0}", precision);
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = precision;
                var tSeconds = Settings.MaxSearchTimeInMinutes*60;
                var tMax = DateTime.Now.Second + tSeconds;
                var updateInterval = Settings.UpdateInterval;
                while (DateTime.Now.Second < tMax)
                {
                    var position = await locator.GetPositionAsync(1000000);
                    LngX = position.Longitude;
                    LatY = position.Latitude;
                    Accuracy = position.Accuracy;
                }
                IsBusy = false;
            }
        }
    }
}
