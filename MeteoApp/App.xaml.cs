namespace MeteoApp;

public partial class App : Application
{
    private static Database _database;
    public static Database database
    {
        get
        {
            if (_database == null)
            {
                try 
                {
                    _database = new Database();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERRORE DB: {ex.Message}");
                    return null; 
                }
            }
            return _database;
        }
    } 

    public App()
    {
       InitializeComponent();
    
        var unit = Preferences.Get("temp_unit", "C");

        MainPage = new AppShell();
    }

 
}