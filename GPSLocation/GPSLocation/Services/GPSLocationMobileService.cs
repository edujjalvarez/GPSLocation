#define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public GPSLocationMobileService()
        {
            _client = new MobileServiceClient(GlobalSettings.AzureUrl);

            var store = new MobileServiceSQLiteStore("gpslocation.db");
            store.DefineTable<Location>();

            //Initializes the SyncContext using the default IMobileServiceSyncHandler.
            _client.SyncContext.InitializeAsync(store);

            _locationTable = _client.GetSyncTable<Location>();
        }

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

        public async Task<IEnumerable<Location>> ReadLocationsAsync()
        {
            return await _locationTable.ReadAsync();
        }

        public async Task<ObservableCollection<Location>> GetLocationItemsAsync(bool syncItems = false)
        {
            try
            {
                if (syncItems)
                {
                    await SyncAsync();
                }
                IEnumerable<Location> items = await _locationTable.ToEnumerableAsync();

                return new ObservableCollection<Location>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task AddOrUpdateLocationAsync(Location location)
        {
            if (string.IsNullOrEmpty(location.Id))
            {
                await _locationTable.InsertAsync(location);
            }
            else
            {
                await _locationTable.UpdateAsync(location);
            }
        }

        public async Task DeleteLocationAsync(Location location)
        {
            await _locationTable.DeleteAsync(location);
        }

        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await _client.SyncContext.PushAsync();

                await _locationTable.PullAsync(
                    //The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                    //Use a different query name for each unique query in your program
                    "allLocationItems",
                    _locationTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling. A real application would handle the various errors like network conditions,
            // server conflicts and others via the IMobileServiceSyncHandler.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        //Update failed, reverting to server's copy.
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change.
                        await error.CancelAndDiscardItemAsync();
                    }
                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }
    }
}
