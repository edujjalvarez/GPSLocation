using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.Model;
using GPSLocation.Model.Entities;
using GPSLocation.Services;
using Newtonsoft.Json;

namespace GPSLocation.ViewModel
{
    public class LocationsViewModel : ObservableBaseObject
    {
        private static string _tag = "LocationsViewModel";

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Location> _locations;
        public ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set { _locations = value; OnPropertyChanged(); }
        }

        public LocationsViewModel()
        {
            Loading = false;
        }

        public async void RefreshLocations()
        {
            Loading = true;
            Locations = await GPSLocationMobileService.Instance.GetLocationItemsAsync();
            Loading = false;
        }
    }
}
