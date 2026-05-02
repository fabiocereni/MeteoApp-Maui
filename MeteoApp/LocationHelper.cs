namespace MeteoApp
{
    public class LocationHelper
    {
        public async Task<Location> getCurrentLocationAsync()
        {
            var permissions = await MainThread.InvokeOnMainThreadAsync(
                () => Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>());

            if (permissions != PermissionStatus.Granted)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    App.Current.MainPage.DisplayAlert(
                        "Permission Denied",
                        "Location permission is required to get the current location.",
                        "OK"));

                permissions = await MainThread.InvokeOnMainThreadAsync(
                    () => Permissions.RequestAsync<Permissions.LocationWhenInUse>());

                if (permissions != PermissionStatus.Granted)
                {
                    var msg = (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                        ? "Please enable location for this app in Settings."
                        : "We'll ask again next time.";

                    await MainThread.InvokeOnMainThreadAsync(() =>
                        App.Current.MainPage.DisplayAlert("Location Required", msg, "OK"));

                    return null;
                }
            }

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                return await Geolocation.Default.GetLocationAsync(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GPS: {ex.Message}");
                return null;
            }
        }
    }
}
