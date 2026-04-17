using System.Collections.ObjectModel;
using System.Globalization;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        private readonly WeatherApiService _apiService;
        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public Command RefreshCommand { get; }

        public ObservableCollection<Entry> Entries
        {
            get => _entries;
            set { _entries = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<Entry>();
            _apiService = new WeatherApiService();
            RefreshCommand = new Command(async () => await RefreshDataAsync());
        }

        private async Task RefreshDataAsync()
        {
            IsRefreshing = true;
            try
            {
                Entries.Clear();
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            if (App.database == null) return;

            try
            {
                IsBusy = true;
                if (Entries.Count > 0) return;

                var existingEntries = await App.database.GetEntriesAsync();

                if (existingEntries.Count == 0)
                {
                    string[] cities = { "London", "New York", "Zurich" };
                    foreach (string city in cities)
                    {
                        try
                        {
                            var weatherData = await _apiService.GetWeatherByCityAsync(city);
                            if (weatherData != null)
                            {
                                var entry = CreateEntryFromWeather(weatherData);
                                await App.database.SaveEntryAsync(entry);
                                Entries.Add(entry);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    foreach (var entry in existingEntries)
                    {
                        Entries.Add(entry);
                    }
                }

                var helper = new LocationHelper();
                var location = await helper.getCurrentLocationAsync();

                if (location != null)
                {
                    try
                    {
                        var weatherDataGps = await _apiService.GetWeatherByLocationAsync(location.Latitude, location.Longitude);
                        if (weatherDataGps != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                var gpsEntry = CreateEntryFromWeather(weatherDataGps);
                                gpsEntry.CityName += " (Current Location)";
                                if (!Entries.Any(e => e.CityName.Contains("(Current Location)")))
                                {
                                    Entries.Insert(0, gpsEntry);
                                }
                            });
                        }
                    }
                    catch { }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task addCityAsync(string cityName)
        {
            try
            {
                var weatherData = await _apiService.GetWeatherByCityAsync(cityName);
                if (weatherData != null)
                {
                    var entry = CreateEntryFromWeather(weatherData);
                    await App.database.SaveEntryAsync(entry);
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Entries.Add(entry);
                    });

                    // Richiama il controllo per la notifica locale
                    await CheckTemperatureAndNotifyAsync(entry);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Errore", $"Non è stato possibile trovare i dati per {cityName}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in addCityAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Errore", "Si è verificato un errore.", "OK");
            }
        }

        public async Task deleteCityAsync(Entry entry)
        {
            await App.database.DeleteEntryAsync(entry);
            Entries.Remove(entry);
        }

        private Entry CreateEntryFromWeather(WeatherResponse data)
        {
            return new Entry
            {
                CityName = $"{data.Name}, {GetCountryName(data.Sys.Country)}",
                Temperature = Math.Round(data.Main.Temp),
                WeatherDescription = data.Weather[0].Description,
                Humidity = data.Main.Humidity,
                WindSpeed = data.Wind.Speed,
                cloudiness = data.Clouds.All,
                WeatherIcon = $"https://openweathermap.org/img/wn/{data.Weather[0].Icon}@4x.png"
            };
        }

        private static string GetCountryName(string countryCode)
        {
            try
            {
                return new RegionInfo(countryCode).DisplayName;
            }
            catch
            {
                return countryCode;
            }
        }

        private async Task CheckTemperatureAndNotifyAsync(Entry entry)
        {
            // Controlla i permessi usando il percorso corretto per la versione 14
            if (await Plugin.LocalNotification.LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await Plugin.LocalNotification.LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            Plugin.LocalNotification.Core.Models.NotificationRequest request = null;

            if (entry.Temperature < 0)
            {
                request = new Plugin.LocalNotification.Core.Models.NotificationRequest
                {
                    NotificationId = Math.Abs(entry.CityName.GetHashCode()),
                    Title = "Allerta Freddo! ❄️",
                    Description = $"{entry.CityName}, {entry.Temperature}°C allerta freddo",
                    Schedule = new Plugin.LocalNotification.Core.Models.NotificationRequestSchedule { NotifyTime = DateTime.Now.AddSeconds(2) }
                };
            }
            else if (entry.Temperature > 25)
            {
                request = new Plugin.LocalNotification.Core.Models.NotificationRequest
                {
                    NotificationId = Math.Abs(entry.CityName.GetHashCode()),
                    Title = "Allerta Caldo! ☀️",
                    Description = $"{entry.CityName}, {entry.Temperature}°C allerta caldo",
                    Schedule = new Plugin.LocalNotification.Core.Models.NotificationRequestSchedule { NotifyTime = DateTime.Now.AddSeconds(2) }
                };
            }

            if (request != null)
            {
                await Plugin.LocalNotification.LocalNotificationCenter.Current.Show(request);
            }
        }
    }
}