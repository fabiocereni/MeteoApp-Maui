using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        private string _apiKey = "YOUR_API_KEY"; // change with your OpenWeatherMap API key

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

            await App.database.DeleteAllEntriesAsync();
            var existingEntries = await App.database.GetEntriesAsync();
            System.Diagnostics.Debug.WriteLine("Existing entries in database after deletion: " + existingEntries.Count);

            foreach (string city in cities)
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";        

                string responseJson = await client.GetStringAsync(url);
                WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);

                Entry entry = new Entry
                {
                    CityName = weatherData.Name,
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

            existingEntries = await App.database.GetEntriesAsync();
            System.Diagnostics.Debug.WriteLine("Entries in database after loading: " + existingEntries.Count);

            LocationHelper helper = new LocationHelper();
            var location = await helper.getCurrentLocationAsync();
            
            if (location != null)
            {
                string urlGps = $"https://api.openweathermap.org/data/2.5/weather?lat={location.Latitude}&lon={location.Longitude}&appid={_apiKey}&units=metric";

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
                                CityName = weatherDataGps.Name + " (Current Location)",
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
    }
}