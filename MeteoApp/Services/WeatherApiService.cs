using System.Net.Http.Json;

namespace MeteoApp
{
    public class WeatherApiService
    {
        private readonly HttpClient _client;
        private string _baseUrl;
        private readonly string _apiKey = "81f6fb625f1378f589f0ae8161bcb951";

        public WeatherApiService()
        {
            _client = new HttpClient();
            InitializeBaseUrl();
        }

        private readonly string _forecastUrl = "https://api.openweathermap.org/data/2.5/forecast";

        private void InitializeBaseUrl()
        {
            _baseUrl = "https://api.openweathermap.org/data/2.5/weather";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                System.Diagnostics.Debug.WriteLine("Piattaforma Android: URL pronto per eventuali test su 10.0.2.2");
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                System.Diagnostics.Debug.WriteLine("Piattaforma iOS: URL pronto per eventuali test su localhost");
            }
        }

        public async Task<WeatherResponse> GetWeatherByCityAsync(string city, string unit = "metric")
        {
            string url = $"{_baseUrl}?q={city}&appid={_apiKey}&units={unit}&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }

        public async Task<WeatherResponse> GetWeatherByLocationAsync(double lat, double lon)
        {
            string url = $"{_baseUrl}?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }

        public async Task<ForecastResponse> GetForecastByCityAsync(string city, string unit = "metric")
        {
            string url = $"{_forecastUrl}?q={city}&appid={_apiKey}&units={unit}&lang=it";
            return await _client.GetFromJsonAsync<ForecastResponse>(url);
        }
    }
}