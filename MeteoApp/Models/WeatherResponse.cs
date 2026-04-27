namespace MeteoApp
{
    public class WeatherResponse
    {
        public string Name { get; set; } // name of the city
        public MainData Main { get; set; } // temperature
        public WeatherData[] Weather { get; set; } // list of weather conditions
        public WindData Wind { get; set; } // wind data
        public CloudsData Clouds { get; set; } // cloud data
        public SysData Sys { get; set; }
    }

    public class MainData
    {
        public double Temp { get; set; }
        public double Temp_Min { get; set; }
        public double Temp_Max { get; set; }
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

    public class SysData
    {
        public string Country { get; set; }
    }

    public class ForecastResponse
    {
        public List<ForecastItem> List { get; set; }
    }

    public class ForecastItem
    {
        public MainData Main { get; set; }
        public WeatherData[] Weather { get; set; }
        public string Dt_Txt { get; set; }
    }

    public class ForecastDay
    {
        public string DayName { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }
}