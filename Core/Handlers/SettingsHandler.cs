using Newtonsoft.Json;

namespace Lyuze.Core.Handlers {
    public class SettingsHandler {
        [JsonProperty("_Discord")]
        public _Discord Discord { get; set; } = new _Discord {
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

        [JsonProperty(nameof(ReactionRoles))]
        public List<ReactionRoleEntry> ReactionRoles { get; set; } = [];

        [JsonProperty(nameof(Database))]
        public Database Database { get; set; } = new Database {
            MongoDb = "Default Mongodb",
            PlayerCollection = "Default PlayerCollection",
            DatabaseName = "Default Database Name"
        };

        private static SettingsHandler? _instance;
        public static SettingsHandler Instance {
            get {
                if (_instance == null)
                    throw new InvalidOperationException("Settings not loaded yet.");
                return _instance;
            }
            private set => _instance = value;
        }

        public static async Task<SettingsHandler> LoadAsync() {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Resources", "Settings", "settings.json");

            SettingsHandler settings;

            if (File.Exists(filePath)) {
                var json = await File.ReadAllTextAsync(filePath);
                settings = JsonConvert.DeserializeObject<SettingsHandler>(json) ?? new SettingsHandler();
            } else {
                settings = new SettingsHandler();
                await SaveSettingsAsync(settings, filePath);
            }

            Instance = settings;  // <-- Assign the singleton instance here

            return Instance;
        }

        private static async Task SaveSettingsAsync(SettingsHandler settings, string filePath) {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task SaveSettingsAsync() {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Resources", "Settings", "settings.json");
            await SaveSettingsAsync(this, filePath);
        }
    }

    public class ApIs {
        [JsonProperty(nameof(Tenor))]
        public string? Tenor { get; set; }

        [JsonProperty("Unsplash Access")]
        public string? UnsplashAccess { get; set; }

        [JsonProperty("Unsplash Secret")]
        public string? UnsplashSecret { get; set; }
    }

    public class _Discord {
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

    // New class for reaction role entries:
    public class ReactionRoleEntry {
        [JsonProperty(nameof(Emoji))]
        public string Emoji { get; set; } = null!;

        [JsonProperty(nameof(RoleId))]
        public ulong RoleId { get; set; }
    }

    public class Database {
        [JsonProperty("MongoDB")]
        public required string MongoDb { get; set; }

        [JsonProperty("Database")]
        public required string DatabaseName {  get; set; }

        [JsonProperty(nameof(PlayerCollection))]
        public required string PlayerCollection {  get; set; }
    }
}
