using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Globalization;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        private string _apiKey = "edf8fe112ff3580933587b76edc24e10"; // change with your OpenWeatherMap API key

        public ObservableCollection<Entry> Entries
        {
            get { return _entries; }
            set { _entries = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<Entry>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            string[] cities = new string[] { "London", "New York", "Zurich"};
            using HttpClient client = new HttpClient();

            // await App.database.DeleteAllEntriesAsync();
            var existingEntries = await App.database.GetEntriesAsync();
            System.Diagnostics.Debug.WriteLine("Existing entries in database after deletion: " + existingEntries.Count);

            if (existingEntries.Count == 0) {
                foreach (string city in cities)
                {
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric&lang=it";        

                    string responseJson = await client.GetStringAsync(url);
                    WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

                    Entry entry = new Entry
                    {
                        CityName = $"{weatherData.Name}, {GetCountryName(weatherData.Sys.Country)}",
                        Temperature = Math.Round(weatherData.Main.Temp),
                        WeatherDescription = weatherData.Weather[0].Description,
                        Humidity = weatherData.Main.Humidity,
                        WindSpeed = weatherData.Wind.Speed,
                        cloudiness = weatherData.Clouds.All,
                        WeatherIcon = $"https://openweathermap.org/img/wn/{weatherData.Weather[0].Icon}@4x.png"
                    };

                    await App.database.SaveEntryAsync(entry);
                    System.Diagnostics.Debug.WriteLine($"Saved entry for {entry.CityName} - {entry.Temperature}°C to database.");
                    Entries.Add(entry);
                }
            } else
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
                string urlGps = $"https://api.openweathermap.org/data/2.5/weather?lat={location.Latitude}&lon={location.Longitude}&appid={_apiKey}&units=metric&lang=it";

                try
                {
                    string responseJson = await client.GetStringAsync(urlGps);
                    WeatherResponse weatherDataGps = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);
                    
                    if (weatherDataGps != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => 
                        {
                            Entries.Insert(0, new Entry
                            {
                                CityName = $"{weatherDataGps.Name}, {GetCountryName(weatherDataGps.Sys.Country)}" + " (Current Location)",
                                Temperature = Math.Round(weatherDataGps.Main.Temp),
                                WeatherDescription = weatherDataGps.Weather[0].Description,

                                Humidity = weatherDataGps.Main.Humidity,
                                WindSpeed = weatherDataGps.Wind.Speed,
                                cloudiness = weatherDataGps.Clouds.All,
                                WeatherIcon = $"https://openweathermap.org/img/wn/{weatherDataGps.Weather[0].Icon}@4x.png"
                            });
                        });
                    }
                }
                catch (HttpRequestException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore API GPS: {ex.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore generico GPS: {ex.Message}");
                }
            }
        }

        public async Task addCityAsync(string cityName)
        {
            using HttpClient client = new HttpClient();
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}&units=metric&lang=it";

            string responseJson = await client.GetStringAsync(url);
            WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

            Entry entry = new Entry
            {
                CityName = $"{weatherData.Name}, {GetCountryName(weatherData.Sys.Country)}",
                Temperature = Math.Round(weatherData.Main.Temp),
                WeatherDescription = weatherData.Weather[0].Description,
                Humidity = weatherData.Main.Humidity,
                WindSpeed = weatherData.Wind.Speed,
                cloudiness = weatherData.Clouds.All,
                WeatherIcon = $"https://openweathermap.org/img/wn/{weatherData.Weather[0].Icon}@4x.png"
            };

            await App.database.SaveEntryAsync(entry);
            System.Diagnostics.Debug.WriteLine($"Saved entry for {entry.CityName} - {entry.Temperature}°C to database.");
            Entries.Add(entry);
        }

        public async Task deleteCityAsync(Entry entry)
        {
            await App.database.DeleteEntryAsync(entry);
            System.Diagnostics.Debug.WriteLine($"Deleted entry for {entry.CityName} from database.");
            Entries.Remove(entry);
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