using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPSLocation.Model.Entities;
using GPSLocation.Utils;
using Xamarin.Forms;

namespace GPSLocation.ViewModel
{
    public class SettingsPageVM : ObservableBaseObject
    {
        private static string _tag = "LocationVM";

        private string _searchTime = "";
        public string SearchTime
        {
            get { return _searchTime; }
            set { _searchTime = value; OnPropertyChanged(); }
        }

        private string _accuracy = "";
        public string Accuracy
        {
            get { return _accuracy; }
            set { _accuracy = value; OnPropertyChanged(); }
        }

        public Command SaveChangesCommand { get; set; }

        public SettingsPageVM()
        {
            SearchTime = Settings.SearchTimeInMinutes.ToString();
            Accuracy = Settings.DesiredAccuracy.ToString();
            SaveChangesCommand = new Command((obj) => SaveChanges());
        }

        void SaveChanges()
        {
            try
            {
                int tempSearchTime = Int32.Parse(SearchTime);
                int tempAccuracy = Int32.Parse(Accuracy);
                if (tempSearchTime < 1)
                {
                    App.Current.MainPage.DisplayAlert("Error", "El tiempo de búsqueda no puede ser menor a 1 minuto", "Aceptar");
                    return;
                }
                if (tempAccuracy < 1)
                {
                    App.Current.MainPage.DisplayAlert("Error", "La precisión no puede ser menor a 1 metro", "Aceptar");
                    return;
                }
                Settings.SearchTimeInMinutes = tempSearchTime;
                Settings.DesiredAccuracy = tempAccuracy;
                //App.Current.MainPage.DisplayAlert("Notificación", "Ajustes guardados correctamente", "Aceptar");
                App.Current.MainPage.SendBackButtonPressed();
            }
            catch (Exception)
            {
                App.Current.MainPage.DisplayAlert("Error", "Los valores ingresados no son válidos", "Aceptar");
            }
        }


    }
}
