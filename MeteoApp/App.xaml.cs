namespace MeteoApp;

public partial class App : Application
{

	// Singleton database instance
	private static Database _database;
	public static Database database
	{
		get
		{
			if (_database == null)
			{
				_database = new Database();
			}
			return _database;
		}
	} 

    public App()
	{
		InitializeComponent();

		MainPage = new MeteoListPage();
	}
}