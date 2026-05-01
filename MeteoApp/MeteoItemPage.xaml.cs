using System.Collections.ObjectModel;
using System.Globalization;

namespace MeteoApp;

[QueryProperty(nameof(Entry), "Entry")]
public partial class MeteoItemPage : ContentPage
{
    private readonly WeatherApiService _apiService = new();

    private Entry _entry;
    public Entry Entry
    {
        get => _entry;
        set
        {
            _entry = value;
            OnPropertyChanged();
            UpdateBlazorParameters();
            if (value != null)
                _ = LoadForecastAsync();
        }
    }

    private ObservableCollection<ForecastDay> _forecast = new();
    public ObservableCollection<ForecastDay> Forecast
    {
        get => _forecast;
        set 
        { 
            _forecast = value; 
            OnPropertyChanged(); 
            UpdateBlazorParameters(); 
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void UpdateBlazorParameters()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (blazorWebView != null && Entry != null)
            {
                blazorWebView.RootComponents.Clear();
                blazorWebView.RootComponents.Add(new Microsoft.AspNetCore.Components.WebView.Maui.RootComponent
                {
                    Selector = "#app",
                    ComponentType = typeof(MeteoDetails),
                    Parameters = new Dictionary<string, object>
                    {
                        { "Entry", Entry },
                        { "Forecast", Forecast }
                    }
                });
            }
        });
    }

    private async Task LoadForecastAsync()
    {
        try
        {
            string queryName = Entry.CityName.Split(',')[0].Trim();
            queryName = queryName.Replace(" (Current Location)", "").Trim();

            var response = await _apiService.GetForecastByCityAsync(queryName);
            if (response?.List == null) return;

            var days = response.List
                .GroupBy(item => DateTime.Parse(item.Dt_Txt, CultureInfo.InvariantCulture).Date)
                .Skip(1)
                .Take(5)
                .Select(g =>
                {
                    var midday = g.OrderBy(i => Math.Abs(
                        DateTime.Parse(i.Dt_Txt, CultureInfo.InvariantCulture).Hour - 12)).First();
                    return new ForecastDay
                    {
                        DayName = g.Key.ToString("ddd", new CultureInfo("it-IT")),
                        TempMin = Math.Round(g.Min(i => i.Main.Temp_Min)),
                        TempMax = Math.Round(g.Max(i => i.Main.Temp_Max)),
                        Icon = $"https://openweathermap.org/img/wn/{midday.Weather[0].Icon}@2x.png",
                        Description = midday.Weather[0].Description
                    };
                });

            Forecast.Clear();
            foreach (var day in days)
                Forecast.Add(day);
            
            UpdateBlazorParameters(); 
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Forecast error: {ex.Message}");
        }
    }
}