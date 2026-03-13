using SQLite;

namespace MeteoApp
{
    public class Entry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CityName { get; set; }
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public int cloudiness { get; set; }
        public double WindSpeed { get; set; }
        public string WeatherIcon { get; set; }
        public string WeatherDescription { get; set; }
    }
}