using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

public partial class MeteoMapPage : ContentPage
{
    public MeteoMapPage()
    {
        InitializeComponent();
        InitializeMap();
    }

    private void InitializeMap()
    {
        // Coordinate Lugano come da dispensa
        var luganoLocation = new Location(46.012, 8.958);
        
        var pin = new Pin
        {
            Label = "SUPSI",
            Address = "Lugano-Viganello",
            Location = luganoLocation
        };

        MyMap.Pins.Add(pin);
        
        var region = MapSpan.FromCenterAndRadius(luganoLocation, Distance.FromKilometers(1));
        MyMap.MoveToRegion(region);
    }
}