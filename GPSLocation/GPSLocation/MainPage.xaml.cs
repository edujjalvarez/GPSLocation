using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.ViewModel;
using Xamarin.Forms;

namespace GPSLocation
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new LocationVM();
        }
    }
}
