using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        private string _apiKey = "YOUR_API_KEY_HERE"; // replace with your OpenWeatherMap API key

        public ObservableCollection<Entry> Entries
        {
            get { return _entries; }
            set { _entries = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<Entry>();

            LoadDataAsync();

            /*
            for (var i = 0; i < 15; i++)
            {
                var e = new Entry
                {
                    Id = i
                };

                Entries.Add(e);
            }*/
        }

        private async void LoadDataAsync()
        {
            string[] cities = new string[] { "London", "New York", "Zurich" };

            using HttpClient client = new HttpClient();

            foreach (string city in cities)
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";        

                string responseJson = await client.GetStringAsync(url); // request to the API the weather data for the city

                WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(responseJson);
                if (weatherData != null)
                {
                    Entries.Add(new Entry
                    {
                        CityName = weatherData.Name,
                        Temperature = weatherData.Main.Temp,
                        WeatherDescription = weatherData.Weather[0].Description
                    });
                }

            
            }
        }

    }
}
