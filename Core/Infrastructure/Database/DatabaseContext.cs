using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Infrastructure.Configuration;
using MongoDB.Driver;

namespace Lyuze.Core.Infrastructure.Database {
    public class DatabaseContext {
        public readonly MongoClient client;
        public readonly IMongoDatabase database;
        public readonly IMongoCollection<PlayerModel> playerCollection;

        private readonly SettingsConfig settings;

        public DatabaseContext(SettingsConfig settingsHandler) {
            settings = settingsHandler ?? throw new ArgumentNullException(nameof(settingsHandler));

            var db = settings.Database ?? throw new InvalidOperationException("Database config missing.");
            client = new MongoClient(db.MongoDb);
            database = client.GetDatabase(db.DatabaseName);
            playerCollection = database.GetCollection<PlayerModel>(db.PlayerCollection);

        }

    }
}