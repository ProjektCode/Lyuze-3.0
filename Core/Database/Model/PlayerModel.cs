using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lyuze.Core.Database.Model {
    public class PlayerModel {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public required ulong DiscordID { get; set; }

        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int XP { get; set; }
        public string Background { get; set; } = string.Empty;
        public bool PublicProfile { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public bool LevelNotify { get; set; }
        public int InfractionCount { get; set; }
        public List<string> InfranctionMessages { get; set; } = [];
    }
}