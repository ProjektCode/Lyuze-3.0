using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Discord.WebSocket;
using Lyuze.Core.Handlers;

namespace Lyuze.Core.Database.Model {
    public class PlayerModel {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public required ulong DiscordID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level {  get; set; }
        public int XP { get; set; }
        public string Background { get; set; } = string.Empty;
        public bool PublicProfile { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public bool LevelNotify { get; set; }
        public int InfractionCount {  get; set; }
        public List<String> InfranctionMessages { get; set; } = new List<String>();
    }

    public class Player {
        public static DatabaseContext Db { get; set; } = null!;

        public static void Initialize(DatabaseContext dbCtx) {
            Db = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
        }


        public static readonly Random random = new();

        public static async Task CreateProfileAsync(SocketGuildUser user) {
            Console.WriteLine($"[ProfileModel] Creacting profile for {user}");
            int num = random.Next(SettingsHandler.Instance.ProfileBanners.Count);

            List<string> messages = [];

            PlayerModel player = new() {
                DiscordID = user.Id,
                UserName = user.Username,
                Level = 1,
                XP = 1,
                Background = SettingsHandler.Instance.ProfileBanners[num].AbsoluteUri,
                AboutMe = "No About me set.",
                LevelNotify = true,
                PublicProfile = true,
                InfractionCount = 0,
                InfranctionMessages = messages
            };

            if(Db == null) return;
            await Db.playerCollection.InsertOneAsync(player);
        }

        public static async Task<bool> HasProfileAsync(SocketGuildUser user) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            return await Db.playerCollection.Find(filter).AnyAsync();
        }



        public static async Task<PlayerModel> GetUserAsync(SocketGuildUser user) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            var u = await Db.playerCollection.Find(filter).FirstOrDefaultAsync();
            return u;
        }

        public static async Task UpdateUserAsync(SocketGuildUser user, PlayerModel p) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            if (Db == null) return;
            await Db.playerCollection.ReplaceOneAsync(filter, p);
        }
    }

}
