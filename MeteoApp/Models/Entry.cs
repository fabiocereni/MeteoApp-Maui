using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeteoApp
{
    public class Entry : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _cityName;
        public string CityName
        {
            get => _cityName;
            set { _cityName = value; OnPropertyChanged(); }
        }

        [Ignore]
        public double TemperatureCelsius { get; set; }

        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set { _temperature = value; OnPropertyChanged(); }
        }

        private int _humidity;
        public int Humidity
        {
            get => _humidity;
            set { _humidity = value; OnPropertyChanged(); }
        }

        private int _cloudiness;
        public int cloudiness
        {
            get => _cloudiness;
            set { _cloudiness = value; OnPropertyChanged(); }
        }

        private double _windSpeed;
        public double WindSpeed
        {
            get => _windSpeed;
            set { _windSpeed = value; OnPropertyChanged(); }
        }

        private string _weatherIcon;
        public string WeatherIcon
        {
            get => _weatherIcon;
            set { _weatherIcon = value; OnPropertyChanged(); }
        }

        private string _weatherDescription;
        public string WeatherDescription
        {
            get => _weatherDescription;
            set { _weatherDescription = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
