using SQLite;
using System.IO;

namespace MeteoApp
{
    public class Database
    {

        private readonly SQLiteAsyncConnection _connection;

        public Database()
        {
            var dataDir = FileSystem.AppDataDirectory;
            var dbPath = Path.Combine(dataDir, "MeteoDb.db");

            string dbEncryptionKey = SecureStorage.GetAsync("dbKey").Result;

            if (string.IsNullOrEmpty(dbEncryptionKey))
            {
                // crea una nuova chiave se non esiste
                dbEncryptionKey = Guid.NewGuid().ToString();
                SecureStorage.SetAsync("dbKey", dbEncryptionKey);
            }

            var dbOptions = new SQLiteConnectionString(dbPath, true, key: dbEncryptionKey);
            // Inizializza la connessione
            _connection = new SQLiteAsyncConnection(dbOptions);

            // Crea la tabella per i nostri dati Meteo (Entry)
            _ = Initialise();
        }

        private async Task Initialise()
        {
            await _connection.CreateTableAsync<Entry>();
            System.Diagnostics.Debug.WriteLine("Database initialized at: " + _connection.DatabasePath);
        }

        public Task<List<Entry>> GetEntriesAsync()
        {
            return _connection.Table<Entry>().ToListAsync();
        }

        public Task<int> SaveEntryAsync(Entry entry)
        {
            if (entry.Id != 0)
            {
                return _connection.UpdateAsync(entry);
            }
            else
            {
                return _connection.InsertAsync(entry);
            }
        }

        public Task<int> DeleteEntryAsync(Entry entry)
        {
            return _connection.DeleteAsync(entry);
        }

        public Task<int> DeleteAllEntriesAsync()
        {
            return _connection.DeleteAllAsync<Entry>();
        }
    }

}