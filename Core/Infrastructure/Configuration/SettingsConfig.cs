using Newtonsoft.Json;

namespace Lyuze.Core.Infrastructure.Configuration {
    public class SettingsConfig {
        [JsonProperty("_Discord")]
        public DiscordStuff Discord { get; set; } = new() {
            Name = "Default Discord Name",
            Token = "Default Discord Token",
            GuildId = 0
        };

        [JsonProperty(nameof(IDs))]
        public IDs? IDs { get; set; }

        [JsonProperty("APIs")]
        public ApIs? ApIs { get; set; }

        [JsonProperty("Image Links")]
        public List<Uri>? ImageLinks { get; set; }

        [JsonProperty("Profile Banners")]
        public List<Uri> ProfileBanners { get; set; } = [];

        [JsonProperty("Welcome Message")]
        public List<string> WelcomeMessage { get; set; } = [];

        [JsonProperty("Goodbye Message")]
        public List<string> GoodbyeMessage { get; set; } = [];

        [JsonProperty(nameof(Status))]
        public List<string> Status { get; set; } = [];

        [JsonProperty(nameof(Database))]
        public Database Database { get; set; } = new() {
            MongoDb = "Default Mongodb",
            PlayerCollection = "Default PlayerCollection",
            DatabaseName = "Default Database Name"
        };

        [JsonProperty(nameof(N8n))]
        public N8N N8n { get; set; } = new() {
            WebhookUrl = string.Empty
        };
    }

    public class ApIs {
        [JsonProperty(nameof(Tenor))]
        public string? Tenor { get; set; }

        [JsonProperty("Unsplash Access")]
        public string? UnsplashAccess { get; set; }

        [JsonProperty("Unsplash Secret")]
        public string? UnsplashSecret { get; set; }

        [JsonProperty("Sauce Nao")]
        public string? SauceNao { get; set; }

        // Optional (higher rate limits): https://danbooru.donmai.us
        [JsonProperty("Danbooru Login")]
        public string? DanbooruLogin { get; set; }

        [JsonProperty("Danbooru Api Key")]
        public string? DanbooruApiKey { get; set; }
    }

    public class DiscordStuff {
        [JsonProperty(nameof(Name))]
        public string? Name { get; set; }

        [JsonProperty(nameof(Token))]
        public required string Token { get; set; }

        [JsonProperty("GuildID")]
        public required ulong GuildId { get; set; }
    }

    public class IDs {
        [JsonProperty("Owner ID")]
        public ulong OwnerId { get; set; }

        [JsonProperty("Welcome ID")]
        public ulong WelcomeId { get; set; }

        [JsonProperty("Report ID")]
        public ulong ReportId { get; set; }

        [JsonProperty("Error ID")]
        public ulong ErrorId { get; set; }

        [JsonProperty("Kick ID")]
        public ulong KickId { get; set; }

        [JsonProperty("Leave ID")]
        public ulong LeaveId { get; set; }

        [JsonProperty("DJ ID")]
        public ulong DjId { get; set; }

        [JsonProperty("Join Role ID")]
        public ulong JoinRoleId { get; set; }

        [JsonProperty("Reaction Roles Message ID")]
        public ulong ReactionRoleMessageId { get; set; }
    }

    public class Database {
        [JsonProperty("MongoDB")]
        public required string MongoDb { get; set; }

        [JsonProperty("Database")]
        public required string DatabaseName { get; set; }

        [JsonProperty(nameof(PlayerCollection))]
        public required string PlayerCollection { get; set; }
    }

    public class N8N {
        // Must match your JSON: "WebhookURL"
        [JsonProperty("WebhookURL")]
        public required string WebhookUrl { get; set; }
    }
}
