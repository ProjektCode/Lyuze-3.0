using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Handlers;
using MongoDB.Driver;

namespace Lyuze.Core.Database {
    public class DatabaseContext {
        public readonly MongoClient client;
        public readonly IMongoDatabase database;
        public readonly IMongoCollection<PlayerModel> playerCollection;

        private readonly SettingsHandler settings;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DatabaseContext(SettingsHandler settingsHandler) {
               settings = settingsHandler ?? throw new ArgumentNullException(nameof(settings));    


                if (settings == null)
                    throw new InvalidOperationException("Database settings not loaded.");

                client = new MongoClient(settings.Database.MongoDb);
                database = client.GetDatabase(settings.Database.DatabaseName);
                playerCollection = database.GetCollection<PlayerModel>(settings.Database.PlayerCollection);
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}