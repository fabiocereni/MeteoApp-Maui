namespace MeteoApp;

public partial class MeteoListPage : Shell
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();

    public MeteoListPage()
    {
        InitializeComponent();
        RegisterRoutes();

        BindingContext = new MeteoListViewModel();
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

    private void OnItemAdded(object sender, EventArgs e)
    {
         _ = ShowPrompt();
    }

    private async void OnItemDeleted(object sender, EventArgs e)
    {
        var menuItem = sender as MenuItem;
        var entry = menuItem.CommandParameter as Entry;

        var viewModel = BindingContext as MeteoListViewModel;
        await viewModel.deleteCityAsync(entry);
    }

    private async Task ShowPrompt()
    {
        string cityName = await DisplayPromptAsync(
            "Add city",
            "Enter the name of the city you want to add:",
            "OK",
            "Cancel",
            "City name",
            -1,
            Keyboard.Text
        );

        if (!string.IsNullOrWhiteSpace(cityName))
        {
            var viewModel = BindingContext as MeteoListViewModel;
            await viewModel.addCityAsync(cityName);
        }
    }
}