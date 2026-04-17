using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

public partial class AddCityPage : ContentPage
{
    private MeteoListViewModel _viewModel;

    public AddCityPage(MeteoListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CenterMapOnUserLocationAsync();
    }

    private async Task CenterMapOnUserLocationAsync()
    {
        LocationHelper helper = new LocationHelper();
        var location = await helper.getCurrentLocationAsync();

        if (location != null)
        {
            var region = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(5));
            SearchMap.MoveToRegion(region);

            SearchMap.IsShowingUser = true;
        }
    }

    private async void OnAddTextClicked(object sender, EventArgs e)
    {
        string cityName = CityNameEntry.Text;

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            await _viewModel.addCityAsync(cityName);
            
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            var placemarks = await Geocoding.GetPlacemarksAsync(e.Location.Latitude, e.Location.Longitude);
            var placemark = placemarks?.FirstOrDefault();

            string cityName = placemark?.Locality ?? placemark?.SubAdminArea ?? placemark?.AdminArea;

            if (!string.IsNullOrWhiteSpace(cityName))
            {
                bool confirm = await DisplayAlert(
                    "Città Trovata", 
                    $"Vuoi aggiungere {cityName}?", 
                    "Sì", 
                    "No");

                if (confirm)
                {
                    await _viewModel.addCityAsync(cityName);
                    
                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                await DisplayAlert("Ops!", "Non sono riuscito a identificare una città in questo punto preciso. Prova a cliccare un po' più vicino al centro abitato.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore nel reverse geocoding: {ex.Message}");
            await DisplayAlert("Errore", $"Si è verificato un problema: {ex.Message}", "OK");
        }
    }
}