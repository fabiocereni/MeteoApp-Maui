using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using MeteoApp;

namespace MeteoApp.Services
{
    public class AppwriteService
    {
        private readonly Databases _databases;

        private static readonly string Endpoint     = Secrets.AppwriteEndpoint;
        private static readonly string ProjectId    = Secrets.AppwriteProjectId;
        private static readonly string DatabaseId   = Secrets.AppwriteDatabaseId;
        private static readonly string CollectionId = Secrets.AppwriteCollectionId;

        public AppwriteService()
        {
            var client = new Client()
                .SetEndpoint(Endpoint)
                .SetProject(ProjectId);

            _databases = new Databases(client);
        }

        public async Task<List<string>> GetCitiesAsync()
        {
            try
            {
                var result = await _databases.ListDocuments(DatabaseId, CollectionId);
                return result.Documents
                    .Select(d => d.Data["name"].ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Appwrite] Errore GetCities: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<string> AddCityAsync(string cityName)
        {
            try
            {
                var doc = await _databases.CreateDocument(
                    DatabaseId,
                    CollectionId,
                    ID.Unique(),
                    new Dictionary<string, object> { { "name", cityName } }
                );
                System.Diagnostics.Debug.WriteLine($"[Appwrite] Città '{cityName}' salvata con ID: {doc.Id}");
                return doc.Id;
            }
            catch (Exception ex)
            {
                string msg = $"Endpoint: {Endpoint}\nProject: {ProjectId}\nDB: {DatabaseId}\nCollection: {CollectionId}\n\nErrore: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Appwrite] ❌ {msg}");
                await App.Current.MainPage.DisplayAlert("Appwrite Errore", msg, "OK");
                return null;
            }
        }

        public async Task<bool> DeleteCityAsync(string documentId)
        {
            try
            {
                await _databases.DeleteDocument(DatabaseId, CollectionId, documentId);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Appwrite] Errore DeleteCity: {ex.Message}");
                return false;
            }
        }
    }
}
