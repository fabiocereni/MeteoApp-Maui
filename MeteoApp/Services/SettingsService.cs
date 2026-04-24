namespace MeteoApp.Services;

public static class SettingsService
{
    private const string TempUnitKey = "temp_unit";

    // "C" per Celsius, "F" per Fahrenheit
    public static string GetTemperatureUnit() => 
        Preferences.Get(TempUnitKey, "C");

    public static void SetTemperatureUnit(string unit) => 
        Preferences.Set(TempUnitKey, unit);
}