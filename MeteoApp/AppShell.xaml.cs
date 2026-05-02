namespace MeteoApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("entrydetails", typeof(MeteoItemPage));
        Routing.RegisterRoute("meteomap", typeof(MeteoMapPage));
    }
}
