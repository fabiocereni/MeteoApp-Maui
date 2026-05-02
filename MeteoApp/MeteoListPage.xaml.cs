namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    public MeteoListPage()
    {
        InitializeComponent();
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