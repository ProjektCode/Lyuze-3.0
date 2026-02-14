using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Models;
using MongoDB.Driver;

namespace Lyuze.Core.Infrastructure.Database {
    public class DatabaseContext {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly SettingsConfig _settings;

        public IMongoCollection<PlayerModel> Players { get; }
        public IMongoCollection<ReactionRoleModel> ReactionRoles { get; }

        public DatabaseContext(SettingsConfig settingsHandler) {
            _settings = settingsHandler ?? throw new ArgumentNullException(nameof(settingsHandler));

            var db = _settings.Database ?? throw new InvalidOperationException("Database config missing.");
            _client = new MongoClient(db.MongoDb);
            _database = _client.GetDatabase(db.DatabaseName);
            Players = _database.GetCollection<PlayerModel>(db.PlayerCollection);
            ReactionRoles = _database.GetCollection<ReactionRoleModel>("ReactionRoles");
        }
    }
}
