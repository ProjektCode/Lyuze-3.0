using Lyuze.Core.Database.Model;
using Lyuze.Core.Handlers;
using MongoDB.Driver;

namespace Lyuze.Core.Database {
    public class DatabaseContext {
        public readonly MongoClient client;
        public readonly IMongoDatabase database;
        public readonly IMongoCollection<PlayerModel> playerCollection;

        private readonly SettingsHandler settings;

        public DatabaseContext(SettingsHandler settingsHandler) {
               settings = settingsHandler ?? throw new ArgumentNullException(nameof(settings));    


                if (settings == null) throw new InvalidOperationException("Database settings not loaded.");

                client = new MongoClient(settings.Database.MongoDb);
                database = client.GetDatabase(settings.Database.DatabaseName);
                playerCollection = database.GetCollection<PlayerModel>(settings.Database.PlayerCollection);
        }
    }
}