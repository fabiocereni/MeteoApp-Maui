using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Microsoft.Maui.Devices.Sensors;

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
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            string[] cities = new string[] { "London", "New York", "Zurich" };
            using HttpClient client = new HttpClient();

            foreach (string city in cities)
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";        

                try 
                {
                    string responseJson = await client.GetStringAsync(url); 
                    WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);
                    
                    if (weatherData != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => 
                        {
                            Entries.Add(new Entry
                            {
                                CityName = weatherData.Name,
                                Temperature = Math.Round(weatherData.Main.Temp),
                                WeatherDescription = weatherData.Weather[0].Description
                            });
                        });
                    }
                }
                catch (HttpRequestException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore API (es. 401) per {city}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore generico per {city}: {ex.Message}");
                }
            }

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
                                WeatherDescription = weatherDataGps.Weather[0].Description
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