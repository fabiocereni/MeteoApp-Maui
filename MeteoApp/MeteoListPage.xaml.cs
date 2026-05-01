namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();

    public MeteoListPage()
    {
        InitializeComponent();
        BindingContext = new MeteoListViewModel();
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
        if (Routes.Count > 0) return;
        Routes.Add("entrydetails", typeof(MeteoItemPage));
        Routes.Add("meteomap", typeof(MeteoMapPage));

        foreach (var item in Routes)
            Routing.RegisterRoute(item.Key, item.Value);
    }

    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Entry entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Entry", entry }
            };
            await Shell.Current.GoToAsync("entrydetails", navigationParameter);
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
        if (sender is MenuItem menuItem && menuItem.CommandParameter is Entry entry)
        {
            if (BindingContext is MeteoListViewModel viewModel)
            {
                await viewModel.deleteCityAsync(entry);
            }
        }
    }
}