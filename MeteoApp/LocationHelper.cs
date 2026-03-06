using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;

namespace MeteoApp
{
    public class LocationHelper
    {
        public async Task<Location> getCurrentLocationAsync()
        {
            var permissions = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (permissions != PermissionStatus.Granted)
            {
                await App.Current.MainPage.DisplayAlert("Permission Denied", "Location permission is required to get the current location.", "OK");

                permissions = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (permissions != PermissionStatus.Granted)
                {
                    if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                        await App.Current.MainPage.DisplayAlert("Location Required", "Location is required to share it. Please enable location for this app in Settings.", "OK");
                    else
                        await App.Current.MainPage.DisplayAlert("Location Required", "Location is required to share it. We'll ask again next time.", "OK");

                    return null;
                }
            }

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);
                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GPS: {ex.Message}");
                return null;
            }
        }
    }
}