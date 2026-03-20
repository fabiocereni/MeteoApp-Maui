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
            get { return _isRefreshing; }
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public Command RefreshCommand { get; }

        public ObservableCollection<Entry> Entries
        {
            get { return _entries; }
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
            Entries.Clear();
            await LoadDataAsync(); 
            IsRefreshing = false;
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            if (Entries.Count > 0) 
                return;

            string[] cities = new string[] { "London", "New York", "Zurich"};

            var existingEntries = await App.database.GetEntriesAsync();
            System.Diagnostics.Debug.WriteLine("Existing entries in database after deletion: " + existingEntries.Count);

            if (existingEntries.Count == 0) 
            {
                foreach (string city in cities)
                {
                    try
                    {
                        WeatherResponse weatherData = await _apiService.GetWeatherByCityAsync(city);

                        if (weatherData != null)
                        {
                            Entry entry = CreateEntryFromWeather(weatherData);
                            await App.database.SaveEntryAsync(entry);
                            System.Diagnostics.Debug.WriteLine($"Saved entry for {entry.CityName} - {entry.Temperature}°C to database.");
                            Entries.Add(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore nel caricamento di {city}: {ex.Message}");
                    }
                }
            } 
            else
            {
                foreach (var entry in existingEntries)
                {
                    Entries.Add(entry);
                }
            }

            existingEntries = await App.database.GetEntriesAsync();
            System.Diagnostics.Debug.WriteLine("Entries in database after loading: " + existingEntries.Count);

            LocationHelper helper = new LocationHelper();
            var location = await helper.getCurrentLocationAsync();
            
            if (location != null)
            {
                try
                {
                    WeatherResponse weatherDataGps = await _apiService.GetWeatherByLocationAsync(location.Latitude, location.Longitude);
                    
                    if (weatherDataGps != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => 
                        {
                            Entry gpsEntry = CreateEntryFromWeather(weatherDataGps);
                            gpsEntry.CityName += " (Current Location)";
                            Entries.Insert(0, gpsEntry);
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore API GPS: {ex.Message}");
                }
            }

            IsBusy = false;
        }

        public async Task addCityAsync(string cityName)
        {
            try
            {
                WeatherResponse weatherData = await _apiService.GetWeatherByCityAsync(cityName);

                if (weatherData != null)
                {
                    Entry entry = CreateEntryFromWeather(weatherData);
                    await App.database.SaveEntryAsync(entry);
                    System.Diagnostics.Debug.WriteLine($"Saved entry for {entry.CityName} - {entry.Temperature}°C to database.");
                    Entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore aggiunta città: {ex.Message}");
            }
        }

        public async Task deleteCityAsync(Entry entry)
        {
            await App.database.DeleteEntryAsync(entry);
            System.Diagnostics.Debug.WriteLine($"Deleted entry for {entry.CityName} from database.");
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
    }
}