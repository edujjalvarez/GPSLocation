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
            throw new NotImplementedException();
        }

        private void LvLocations_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LvLocations_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
