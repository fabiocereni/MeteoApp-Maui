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

    // --- 1. Quando la pagina appare, cerchiamo la posizione dell'utente ---
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CenterMapOnUserLocationAsync();
    }

    private async Task CenterMapOnUserLocationAsync()
    {
        // Usiamo il tuo LocationHelper per gestire i permessi e ottenere la posizione
        LocationHelper helper = new LocationHelper();
        var location = await helper.getCurrentLocationAsync();

        if (location != null)
        {
            // Centriamo la mappa sulle tue coordinate (con un raggio di 5 km)
            var region = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(5));
            SearchMap.MoveToRegion(region);

            // Abilitiamo il "puntino blu" della posizione utente come da istruzioni del lab
            SearchMap.IsShowingUser = true;
        }
    }

    // --- 2. Metodo per l'aggiunta tramite il testo digitato ---
    private async void OnAddTextClicked(object sender, EventArgs e)
    {
        string cityName = CityNameEntry.Text;

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            await _viewModel.addCityAsync(cityName);
            
            // Chiudiamo la pagina modale
            await Navigation.PopModalAsync();
        }
    }

    // --- 3. Metodo per annullare l'operazione ---
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        // Chiudiamo la pagina modale senza fare nulla
        await Navigation.PopModalAsync();
    }

    // --- 4. Metodo per l'aggiunta tramite il "ping" (Tap) sulla mappa ---
    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            // Il Reverse Geocoding trasforma le coordinate toccate (e.Location) in un indirizzo/città
            var placemarks = await Geocoding.GetPlacemarksAsync(e.Location.Latitude, e.Location.Longitude);
            var placemark = placemarks?.FirstOrDefault();

            // Placemark contiene molte info, "Locality" è solitamente il nome della città
            if (placemark != null && !string.IsNullOrWhiteSpace(placemark.Locality))
            {
                string cityName = placemark.Locality;
                
                // Chiediamo conferma all'utente con un pop-up
                bool confirm = await DisplayAlert(
                    "Città Trovata", 
                    $"Vuoi aggiungere {cityName}?", 
                    "Sì", 
                    "No");

                if (confirm)
                {
                    // Chiamiamo l'API con il nome della città trovata
                    await _viewModel.addCityAsync(cityName);
                    
                    // Chiudiamo la pagina modale
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
            await DisplayAlert("Errore", "Si è verificato un problema nel recuperare le informazioni del luogo.", "OK");
        }
    }
}