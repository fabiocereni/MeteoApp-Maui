using System.Collections.ObjectModel;
using System.Globalization;

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
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Errore", $"Non è stato possibile trovare i dati meteo per {cityName}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in addCityAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Errore", "Si è verificato un errore durante l'aggiunta della città.", "OK");
            }
        }

        public async Task deleteCityAsync(Entry entry)
        {
            await App.database.DeleteEntryAsync(entry);
            Entries.Remove(entry);
        }

        private Entry CreateEntryFromWeather(WeatherResponse data)
        {
            System.Diagnostics.Debug.WriteLine($"Città: {data.Name}, Nazione: {data.Sys.Country}");
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
    }
}