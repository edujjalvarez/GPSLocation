using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.ViewModel;
using Xamarin.Forms;

namespace GPSLocation.View
{
    public partial class LocationsView : ContentPage
    {
        private readonly LocationsViewModel _locationsViewModel;

        public LocationsView()
        {
            _locationsViewModel = new LocationsViewModel();
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _locationsViewModel.RefreshLocations();
            BindingContext = _locationsViewModel;
        }

        private void BtnLocationSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LvLocations.BeginRefresh();
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                LvLocations.ItemsSource = _locationsViewModel.Locations;
            }
            else
            {
                LvLocations.ItemsSource = _locationsViewModel.Locations.Where(location =>
                {
                    //Log.GetInstance().Print(_tag, "location.Description = {0}", location.Description);
                    //Log.GetInstance().Print(_tag, "location.Latitude = {0}", location.Latitude.ToString());
                    //Log.GetInstance().Print(_tag, "location.Longitude = {0}", location.Longitude.ToString());
                    try
                    {
                        return (location.Description.ToLower().Contains(e.NewTextValue.Trim().ToLower())) ||
                                (location.Latitude.ToString().ToLower().Contains(e.NewTextValue.Trim().ToLower())) ||
                                location.Longitude.ToString().ToLower().Contains(e.NewTextValue.Trim().ToLower());
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });
            }
            LvLocations.EndRefresh();
        }

        private void LvLocations_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            
        }

        private void LvLocations_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            LvLocations.SelectedItem = null;
        }
    }
}
