using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using GPSLocation.Model;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;

namespace GPSLocation.Services
{
    public class GPSLocationMobileService
    {
        private MobileServiceClient _client;
        private IMobileServiceSyncTable<Location> _locationTable;
        private static GPSLocationMobileService _instance;

        public static GPSLocationMobileService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GPSLocationMobileService();
                }

                return _instance;
            }
        }

        public async Task InitializeAsync()
        {
            if (_client != null)
                return;

            // Inicialización de SQLite local
            var store = new MobileServiceSQLiteStore("gpslocation.db");
            store.DefineTable<Location>();

            _client = new MobileServiceClient(GlobalSettings.AzureUrl);
            _locationTable = _client.GetSyncTable<Location>();

            //Inicializa the utilizando IMobileServiceSyncHandler.
            await _client.SyncContext.InitializeAsync(store,
                new MobileServiceSyncHandler());

            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    // Subir cambios a la base de datos remota
                    await _client.SyncContext.PushAsync();

                    await _locationTable.PullAsync(
                        "allLocationItems", _locationTable.CreateQuery());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: {0}", ex.Message);
                }
            }
        }

        public async Task<IEnumerable<Location>> ReadLocationsAsync()
        {
            await InitializeAsync();
            return await _locationTable.ReadAsync();
        }

        public async Task AddOrUpdateLocationAsync(Location location)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(location.Id))
            {
                await _locationTable.InsertAsync(location);
            }
            else
            {
                await _locationTable.UpdateAsync(location);
            }

            await SynchronizeLocationAsync(location.Id);
        }

        public async Task DeleteLocationAsync(Location location)
        {
            await InitializeAsync();

            await _locationTable.DeleteAsync(location);

            await SynchronizeLocationAsync(location.Id);
        }

        private async Task SynchronizeLocationAsync(string xamagramItemId)
        {
            if (!CrossConnectivity.Current.IsConnected)
                return;

            try
            {
                // Subir cambios a la base de datos remota
                await _client.SyncContext.PushAsync();

                // El primer parámetro es el nombre de la query utilizada intermanente por el client SDK para implementar sync.
                // Utiliza uno diferente por cada query en la App
                await _locationTable.PullAsync("syncLocationItem" + xamagramItemId,
                    _locationTable.Where(r => r.Id == xamagramItemId));
            }
            catch (MobileServicePushFailedException ex)
            {
                if (ex.PushResult != null)
                {
                    foreach (var result in ex.PushResult.Errors)
                    {
                        await ResolveErrorAsync(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción: {0}", ex.Message);
            }
        }

        private async Task ResolveErrorAsync(MobileServiceTableOperationError result)
        {
            // Ignoramos si no podemos validar ambas partes.
            if (result.Result == null || result.Item == null)
                return;

            var serverItem = result.Result.ToObject<Location>();
            var localItem = result.Item.ToObject<Location>();

            if (serverItem.Description == localItem.Description
                && serverItem.Id == localItem.Id)
            {
                // Los elementos sin iguales, ignoramos el conflicto
                await result.CancelAndDiscardItemAsync();
            }
            else
            {
                // Para nosotros, gana el cliente
                localItem.AzureVersion = serverItem.AzureVersion;
                await result.UpdateOperationAsync(JObject.FromObject(localItem));
            }
        }
    }
}
