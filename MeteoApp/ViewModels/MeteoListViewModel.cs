using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using MeteoApp.Services;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        private readonly WeatherApiService _apiService;
        private bool _isRefreshing;
        private string _tempUnit;

        public string TempUnit
        {
            get => _tempUnit;
            set
            {
                if (_tempUnit != value)
                {
                    _tempUnit = value;
                    SettingsService.SetTemperatureUnit(value);
                    OnPropertyChanged();
                    _ = LoadDataAsync(); 
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public Command RefreshCommand { get; }

        public ObservableCollection<Entry> Entries
        {
            get => _entries;
            set { _entries = value; OnPropertyChanged(); }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<Entry>();
            _apiService = new WeatherApiService();
            RefreshCommand = new Command(async () => await RefreshDataAsync());
        }

        private async Task RefreshDataAsync()
        {
            IsRefreshing = true;
            try
            {
                Entries.Clear();
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            if (App.database == null) return;

            try
            {
                IsBusy = true;

                var existingEntries = await App.database.GetEntriesAsync();

                if (existingEntries.Count == 0)
                {
                    string[] cities = { "London", "New York", "Zurich" };
                    foreach (string city in cities)
                    {
                        try
                        {
                            var weatherData = await _apiService.GetWeatherByCityAsync(city);
                            if (weatherData != null)
                            {
                                var entry = CreateEntryFromWeather(weatherData);
                                await App.database.SaveEntryAsync(entry);
                                Entries.Add(entry);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    if (Entries.Count == 0)
                    {
                        foreach (var entry in existingEntries)
                        {
                            Entries.Add(entry);
                        }
                    }

                    foreach (var entry in existingEntries)
                    {
                        try
                        {
                            string queryName = entry.CityName.Split(',')[0].Trim();
                            var weatherData = await _apiService.GetWeatherByCityAsync(queryName);
                            if (weatherData != null)
                            {
                                var updated = CreateEntryFromWeather(weatherData);
                                
                                var entryToUpdate = Entries.FirstOrDefault(e => e.Id == entry.Id);
                                if (entryToUpdate != null)
                                {
                                    entryToUpdate.Temperature = updated.Temperature;
                                    entryToUpdate.WeatherDescription = updated.WeatherDescription;
                                    entryToUpdate.WeatherIcon = updated.WeatherIcon;
                                    entryToUpdate.Humidity = updated.Humidity;
                                    entryToUpdate.WindSpeed = updated.WindSpeed;
                                    entryToUpdate.cloudiness = updated.cloudiness;

                                    await App.database.SaveEntryAsync(entryToUpdate);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Errore durante l'aggiornamento di {entry.CityName}: {ex.Message}");
                        }
                    }
                }

                var helper = new LocationHelper();
                var location = await helper.getCurrentLocationAsync();

                if (location != null)
                {
                    try
                    {
                        var weatherDataGps = await _apiService.GetWeatherByLocationAsync(location.Latitude, location.Longitude);
                        if (weatherDataGps != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                var gpsEntry = CreateEntryFromWeather(weatherDataGps);
                                gpsEntry.CityName += " (Current Location)";
                                
                                var existingGps = Entries.FirstOrDefault(e => e.CityName.Contains("(Current Location)"));
                                if (existingGps != null)
                                {
                                    existingGps.Temperature = gpsEntry.Temperature;
                                    existingGps.WeatherDescription = gpsEntry.WeatherDescription;
                                    existingGps.WeatherIcon = gpsEntry.WeatherIcon;
                                    existingGps.Humidity = gpsEntry.Humidity;
                                    existingGps.WindSpeed = gpsEntry.WindSpeed;
                                    existingGps.cloudiness = gpsEntry.cloudiness;
                                }
                                else
                                {
                                    Entries.Insert(0, gpsEntry);
                                }
                            });
                        }
                    }
                    catch { }
                }

                // INVIA I DATI AL SERVER NODE.JS DOPO AVER CARICATO TUTTO
                await RegisterTokenWithServerAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task addCityAsync(string cityName)
        {
            try
            {
                var weatherData = await _apiService.GetWeatherByCityAsync(cityName);
                if (weatherData != null)
                {
                    var entry = CreateEntryFromWeather(weatherData);
                    await App.database.SaveEntryAsync(entry);
                    
                    // Aggiungiamo la città alla lista aggiornando la UI
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Entries.Add(entry);
                    });

                    // AVVISA IL SERVER NODE.JS CHE C'È UNA NUOVA CITTÀ DA MONITORARE
                    await RegisterTokenWithServerAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Errore", $"Non è stato possibile trovare i dati per {cityName}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in addCityAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Errore", "Si è verificato un errore.", "OK");
            }
        }

        public async Task deleteCityAsync(Entry entry)
        {
            await App.database.DeleteEntryAsync(entry);
            Entries.Remove(entry);
            
            // AVVISA IL SERVER NODE.JS CHE ABBIAMO ELIMINATO UNA CITTÀ
            await RegisterTokenWithServerAsync();
        }

        private Entry CreateEntryFromWeather(WeatherResponse data)
        {
            return new Entry
            {
                CityName = $"{data.Name}, {GetCountryName(data.Sys.Country)}",
                Temperature = Math.Round(data.Main.Temp),
                WeatherDescription = data.Weather[0].Description,
                Humidity = data.Main.Humidity,
                WindSpeed = data.Wind.Speed,
                cloudiness = data.Clouds.All,
                WeatherIcon = $"https://openweathermap.org/img/wn/{data.Weather[0].Icon}@4x.png"
            };
        }

        private static string GetCountryName(string countryCode)
        {
            try
            {
                return new RegionInfo(countryCode).DisplayName;
            }
            catch
            {
                return countryCode;
            }
        }

        // METODO PER SINCRONIZZARE IL TELEFONO COL SERVER NODE.JS
        private async Task RegisterTokenWithServerAsync()
        {
            try
            {
#if ANDROID || IOS
                // Verifica che Firebase sia pronto e ottieni il Token univoco del telefono
                await Plugin.Firebase.CloudMessaging.CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                var token = await Plugin.Firebase.CloudMessaging.CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                // Estrai solo i nomi puliti delle città dalla tua lista (rimuovendo la nazione e la dicitura GPS)
                var cityNames = Entries
                    .Select(e => e.CityName.Split(',')[0].Replace(" (Current Location)", "").Trim())
                    .Distinct()
                    .ToList();

                var payload = new
                {
                    token = token,
                    cities = cityNames
                };

                // Invia i dati al tuo Server Node.js locale
                using var client = new HttpClient();
                
                // Se sei su Android usa 10.0.2.2, se sei su iOS (Simulatore Mac) usa localhost
                var url = DeviceInfo.Platform == DevicePlatform.Android 
                    ? "http://10.0.2.2:3000/register" 
                    : "http://localhost:3000/register";

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                await client.PostAsync(url, content);
                System.Diagnostics.Debug.WriteLine($"Lista città inviata al server Node.js ({cityNames.Count} città)");
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore di sync con il server Node.js: {ex.Message}");
            }
        }
    }
}