using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GPSLocation.Model;
using GPSLocation.Model.Entities;
using GPSLocation.Services;
using GPSLocation.Utils;
using GPSLocation.View;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace GPSLocation.ViewModel
{
    public class LocationVM : ObservableBaseObject
    {
        private static string _tag = "LocationVM";
        private MediaFile _photoMediaFile;
        private Location _location;

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

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _loadingText = "";
        public string LoadingText
        {
            get { return _loadingText; }
            set { _loadingText = value; OnPropertyChanged(); }
        }

        private bool _isEnableStartButton = true;
        public bool IsEnableStartButton
        {
            get { return _isEnableStartButton; }
            set { _isEnableStartButton = value; OnPropertyChanged(); }
        }

        private bool _isEnableStopButton = false;
        public bool IsEnableStopButton
        {
            get { return _isEnableStopButton; }
            set { _isEnableStopButton = value; OnPropertyChanged(); }
        }

        private bool _isStoppedGPSSearch = false;
        public bool IsStoppedGPSSearch
        {
            get { return _isStoppedGPSSearch; }
            set { _isStoppedGPSSearch = value; OnPropertyChanged(); }
        }

        private string _imageIndicator = "";
        public string ImageIndicator
        {
            get { return _imageIndicator; }
            set { _imageIndicator = value; OnPropertyChanged(); }
        }

        private bool _wasFoundLocation = false;
        public bool WasFoundLocation
        {
            get { return _wasFoundLocation; }
            set { _wasFoundLocation = value; OnPropertyChanged(); }
        }

        private string _description = "";
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(); }
        }

        public Command StartSearchGPSCommand { get; set; }
        public Command StopSearchGPSCommand { get; set; }
        public Command GoToSettingsPageCommand { get; set; }
        public Command GoToLocationsViewCommand { get; set; }
        public ICommand SaveLocationCommand => new Command(async () => await SaveLocationAsync());
        public ICommand TakePhotoCommand => new Command(async () => await TakePhotoAsync());

        private async Task TakePhotoAsync()
        {
            try
            {
                var cameraPermission = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (cameraPermission != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                    {
                        await App.Current.MainPage.DisplayAlert("Atención", "App1 necesita permiso para acceder a la cámara", "OK");
                    }

                    var permissionResults = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                    cameraPermission = permissionResults[Permission.Camera];
                }

                if (cameraPermission != PermissionStatus.Granted && cameraPermission != PermissionStatus.Unknown)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Permiso de cámara denegado.", "OK");
                    return;
                }

                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Cámara no disponible.", "OK");
                    return;
                }

                IsLoading = true;
                LoadingText = "Abriendo cámara...";

                string photoFileName = Guid.NewGuid() + ".jpg";
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    //Directory = "xamarin",
                    SaveToAlbum = true,
                    Name = photoFileName,
                    CompressionQuality = 92,
                    PhotoSize = PhotoSize.Small
                });

                LoadingText = "Guardando foto...";

                if (file == null)
                    return;

                Log.GetInstance().Print(_tag, "BtnTomarFotoClick >>> file.AlbumPath = {0}", file.AlbumPath);
                Log.GetInstance().Print(_tag, "BtnTomarFotoClick >>> file.Path = {0}", file.Path);

                _photoMediaFile = file;
                
                CloudStorageAccount csa = CloudStorageAccount.Parse(
                    "YOUR_AZURE_STORAGE_ACCOUNT_CONNECTION_STRING");
                CloudBlobClient blobClient = csa.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference("xamarin");
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(photoFileName);
                await blockBlob.UploadFromStreamAsync(_photoMediaFile.GetStream());
                _location.Image = blockBlob.Uri.ToString();
                IsLoading = false;
                LoadingText = "";
                await App.Current.MainPage.DisplayAlert("Notificación", "Foto guardada correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }

        }

        public LocationVM()
        {
            Log.GetInstance().Print(_tag, "LocationVM() >>> Se llama al constructor.");
            IsLoading = false;
            IsEnableStartButton = true;
            IsEnableStopButton = false;
            WasFoundLocation = false;
            LngX = 0;
            LatY = 0;
            Accuracy = 0.00;
            ImageIndicator = "sred.png";
            IsStoppedGPSSearch = false;
            StartSearchGPSCommand = new Command((obj) => StartSearchGPS());
            StopSearchGPSCommand = new Command((obj) => StopSearchGPS());
            GoToSettingsPageCommand = new Command((obj) => GoToSettingsPage());
            GoToLocationsViewCommand = new Command((obj) => GoToLocationsView());
            DisableStopButton(500);
        }

        private void ReinitGPSVariables()
        {
            IsLoading = true;
            IsEnableStartButton = false;
            IsEnableStopButton = false;
            WasFoundLocation = false;
            LngX = 0;
            LatY = 0;
            Accuracy = 0.00;
            ImageIndicator = "sred.png";
            IsStoppedGPSSearch = false;
        }

        private async void StartSearchGPS()
        {
            if (!IsLoading)
            {
                LoadingText = "Buscando...";
                ReinitGPSVariables();
                int precision = Settings.DesiredAccuracy;
                Log.GetInstance().Print(_tag, "StartSearchGPS() >>> Precisión deseada = {0}", precision);
                var locator = CrossGeolocator.Current;
                if (!locator.IsGeolocationAvailable)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Tu dispositivo no tiene soporte GPS", "Aceptar");
                    IsLoading = false;
                    IsEnableStartButton = true;
                    IsEnableStopButton = false;
                    IsStoppedGPSSearch = false;
                    return;
                }
                if (!locator.IsGeolocationEnabled)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "GPS no activado", "Aceptar");
                    IsLoading = false;
                    IsEnableStartButton = true;
                    IsEnableStopButton = false;
                    IsStoppedGPSSearch = false;
                    return;
                }
                _location = new Location();
                locator.DesiredAccuracy = precision;
                int searchTimeInSeconds = Settings.SearchTimeInMinutes*60;
                DateTime startDateTime = DateTime.Now;
                DateTime finishDateTime = DateTime.Now.AddSeconds(searchTimeInSeconds);
                //int updateInterval = Settings.UpdateInterval;
                bool isFirstTime = true;
                while (startDateTime < finishDateTime && !IsStoppedGPSSearch)
                {
                    DateTime tempStart = DateTime.Now;
                    Position position = null;
                    try
                    {
                        position = await locator.GetPositionAsync(searchTimeInSeconds*1000);
                    }
                    catch (Exception ex)
                    {
                       Log.GetInstance().Print(_tag, "StartSearchGPS() >>> Error obteniendo ubicación. Detalle: {0}", ex.Message);
                    }
                    DateTime tempFinish = DateTime.Now;
                    TimeSpan tempDiff = tempFinish - tempStart;
                    startDateTime = startDateTime.AddSeconds(tempDiff.TotalSeconds);
                    if (isFirstTime)
                    {
                        ImageIndicator = "syellow.png";
                        LngX = position?.Longitude ?? 0.00;
                        LatY = position?.Latitude ?? 0.00;
                        Accuracy = Math.Round(position?.Accuracy ?? 0.00, 2);
                        isFirstTime = false;
                        if (Math.Abs(Accuracy) > 0 && Accuracy < precision)
                        {
                            ImageIndicator = "sgreen.png";
                            IsEnableStopButton = true;
                            WasFoundLocation = true;
                        }
                    }
                    else
                    {
                        if ((position != null && position.Accuracy < Accuracy) || (position != null && Math.Abs(Accuracy) < 1))
                        {
                            LngX = position.Longitude;
                            LatY = position.Latitude;
                            Accuracy = Math.Round(position.Accuracy, 2);
                            if (Math.Abs(Accuracy) > 0 && Accuracy < precision)
                            {
                                ImageIndicator = "sgreen.png";
                                IsEnableStopButton = true;
                                WasFoundLocation = true;
                            }
                        }
                    }
                }
                IsLoading = false;
                IsEnableStartButton = true;
                IsEnableStopButton = false;
                IsStoppedGPSSearch = false;
            }
        }

        private void StopSearchGPS()
        {
            LoadingText = "Deteniendo búsqueda...";
            IsStoppedGPSSearch = true;
        }

        private void GoToSettingsPage()
        {
            App.Current.MainPage.Navigation.PushAsync(new SettingsPage());
        }

        private void GoToLocationsView()
        {
            App.Current.MainPage.Navigation.PushAsync(new LocationsView());
        }

        private async Task DisableStopButton(int milliseconds)
        {
            await Task.Delay(milliseconds);
            IsEnableStopButton = false;
        }

        private async Task SaveLocationAsync()
        {
            if (Description.Trim().Equals(""))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ingrese una descripción para la ubicación.", "Aceptar");
                return;
            }
            IsLoading = true;
            LoadingText = "Guardando ubicación...";
            _location.Description = Description;
            _location.Latitude = LatY;
            _location.Longitude = LngX;
            await GPSLocationMobileService.Instance.AddOrUpdateLocationAsync(_location);
            await GPSLocationMobileService.Instance.GetLocationItemsAsync(true);
            WasFoundLocation = false;
            IsLoading = false;
            LoadingText = "";
            await Application.Current.MainPage.DisplayAlert("Notificación", "Ubicación guardada correctamente.", "Aceptar");
        }

    }
}
