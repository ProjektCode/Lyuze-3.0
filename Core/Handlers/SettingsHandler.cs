using Newtonsoft.Json;

namespace Lyuze.Core.Handlers {
    public partial class SettingsHandler {
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
        public List<Uri>? ProfileBanners { get; set; }

        [JsonProperty("Welcome Message")]
        public List<string>? WelcomeMessage { get; set; }

        [JsonProperty("Goodbye Message")]
        public List<string>? GoodbyeMessage { get; set; }

        [JsonProperty(nameof(Status))]
        public List<string>? Status { get; set; }

        [JsonProperty(nameof(ReactionRoles))]
        public List<ReactionRoleEntry> ReactionRoles { get; set; } = [];

        [JsonProperty(nameof(Database), NullValueHandling = NullValueHandling.Ignore)]
        public Database? Database { get; set; }

        private static readonly Lazy<SettingsHandler> _instance = new(() => {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Resources", "Settings", "settings.json");
            return LoadFromFile(filePath);
        });

        public static SettingsHandler Instance => _instance.Value;

        // Optional utility method for manual data loading (private)
        private static SettingsHandler LoadFromFile(string filePath) {
            if (File.Exists(filePath)) {
                var json = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<SettingsHandler>(json);
                return settings ?? new SettingsHandler();
            } else {
                var settings = new SettingsHandler();
                SaveSettings(settings, filePath);
                return settings;
            }
        }


        public void SaveSettings() {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Resources", "Settings", "settings.json");
            SaveSettings(this, filePath);
        }

        private static void SaveSettings(SettingsHandler settings, string filePath) {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }

    // Supporting classes below

    public partial class ApIs {
        [JsonProperty(nameof(Tenor))]
        public string? Tenor { get; set; }

        [JsonProperty("Unsplash Access")]
        public string? UnsplashAccess { get; set; }

        [JsonProperty("Unsplash Secret")]
        public string? UnsplashSecret { get; set; }
    }

    public partial class _Discord {
        [JsonProperty(nameof(Name))]
        public string? Name { get; set; }

        [JsonProperty(nameof(Token))]
        public required string Token { get; set; }

        [JsonProperty("GuildID")]
        public required ulong GuildId { get; set; }
    }

    public partial class IDs {
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

    public partial class Database {
        [JsonProperty("MongoDB")]
        public string? MongoDb { get; set; }
    }
}
