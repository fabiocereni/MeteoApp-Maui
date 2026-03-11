namespace MeteoApp
{
    public class WeatherResponse
    {
        public string Name { get; set; } // name of the city
        public MainData Main { get; set; } // temperature
        public WeatherData[] Weather { get; set; } // list of weather conditions
        public WindData Wind { get; set; } // wind data
        public CloudsData Clouds { get; set; } // cloud data
    }

    public class MainData
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
    }

    public class WindData
    {
        public double Speed { get; set; }
    }

    public class CloudsData
    {
        public int All { get; set; }
    }

    public class WeatherData
    {
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}