namespace MeteoApp;

public partial class MeteoListPage : Shell
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();
    public MeteoListPage()
    {
        InitializeComponent();
        
        var viewModel = new MeteoListViewModel();
        BindingContext = viewModel;
        
        RegisterRoutes();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MeteoListViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }

    private void RegisterRoutes()
    {
        Routes.Add("entrydetails", typeof(MeteoItemPage));
        Routes.Add("meteomap", typeof(MeteoMapPage));

        foreach (var item in Routes)
            Routing.RegisterRoute(item.Key, item.Value);
    }

    private void OnCardTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Entry entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Entry", entry }
            };

            Shell.Current.GoToAsync($"entrydetails", navigationParameter);
        }
    }

    private async void OnItemAdded(object sender, EventArgs e)
    {
        if (BindingContext is MeteoListViewModel viewModel)
        {
            await Navigation.PushModalAsync(new AddCityPage(viewModel));
        }
    }

    private async void OnItemDeleted(object sender, EventArgs e)
    {
        var menuItem = sender as MenuItem;
        var entry = menuItem.CommandParameter as Entry;

        var viewModel = BindingContext as MeteoListViewModel;
        await viewModel.deleteCityAsync(entry);
    }
}