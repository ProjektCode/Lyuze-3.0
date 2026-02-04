using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lyuze.Core.Models;

public class ReactionRoleModel {
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("emoji")]
    public required string Emoji { get; set; }

    [BsonElement("roleId")]
    public required ulong RoleId { get; set; }

    [BsonElement("messageId")]
    public required ulong MessageId { get; set; }
}
