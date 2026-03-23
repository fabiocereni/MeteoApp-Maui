using System.Net.Http.Json;

namespace MeteoApp
{
    public class WeatherApiService
    {
        private readonly HttpClient _client;
        private string _baseUrl;
        private readonly string _apiKey = "edf8fe112ff3580933587b76edc24e10";

        public WeatherApiService()
        {
            _client = new HttpClient();
            InitializeBaseUrl();
        }

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

        public async Task<WeatherResponse> GetWeatherByCityAsync(string city)
        {
            string url = $"{_baseUrl}?q={city}&appid={_apiKey}&units=metric&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }

        public async Task<WeatherResponse> GetWeatherByLocationAsync(double lat, double lon)
        {
            string url = $"{_baseUrl}?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=it";
            return await _client.GetFromJsonAsync<WeatherResponse>(url);
        }
    }
}