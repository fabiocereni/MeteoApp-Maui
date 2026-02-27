namespace MeteoApp
{
    public class WeatherResponse
    {
        public string Name { get; set; } // name of the city
        public MainData Main { get; set; } // temperature
        public List<WeatherInfo> Weather { get; set; } // list of weather conditions (description)
    }

    public class MainData
    {
        public double Temp { get; set; }
    }

    public class WeatherInfo
    {
        public string Description { get; set; }
    }
}