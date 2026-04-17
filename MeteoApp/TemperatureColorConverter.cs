using System.Globalization;

namespace MeteoApp
{
    public class TemperatureColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double temperature)
            {
                if (temperature <= 15)
                    return Application.Current.Resources["TemperatureCold"];
                else
                    return Application.Current.Resources["TemperatureHot"];
            }
            return Application.Current.Resources["WeatherCloudy"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}