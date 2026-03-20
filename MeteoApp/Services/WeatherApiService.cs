using System.Net.Http.Json; // Necessario per utilizzare GetFromJsonAsync

namespace MeteoApp
{
    public class WeatherApiService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey = "edf8fe112ff3580933587b76edc24e10"; // change with your OpenWeatherMap API key

        public WeatherApiService()
        {
            _client = new HttpClient();
        }

        public async Task<WeatherResponse> GetWeatherByCityAsync(string city)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }

        public async Task<WeatherResponse> GetWeatherByLocationAsync(double lat, double lon)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }
    }
}