using Discord.WebSocket;
using MongoDB.Driver;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Infrastructure.Database;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Features.Profiles {
    public sealed class PlayerService(DatabaseContext db, SettingsConfig settings, ILoggingService logger) : IPlayerService {
        private readonly DatabaseContext _db = db;
        private readonly SettingsConfig _settings = settings;
        private readonly ILoggingService _logger = logger;
        private readonly Random _random = new();

        public async Task CreateProfileAsync(SocketGuildUser user) {
            await _logger.LogInformationAsync("profile", $"Creating profile for {user}");

            var banners = _settings.ProfileBanners;
            if (banners == null || banners.Count == 0) {
                await _logger.LogWarningAsync("profile",
                    "ProfileBanners is empty. Using default background.");
            }

            // Pick a banner or use a default
            string backgroundUrl =
                banners != null && banners.Count > 0
                    ? banners[_random.Next(banners.Count)].AbsoluteUri
                    : "https://images.unsplash.com/photo-1433259651738-0e74537aa8b5?auto=format&fit=crop&w=1469&q=80";

            var player = new PlayerModel {
                DiscordID = user.Id,
                UserName = user.Username,
                Level = 1,
                XP = 1,
                Background = backgroundUrl,
                AboutMe = "No About me set.",
                LevelNotify = true,
                PublicProfile = true,
                InfractionCount = 0,
                InfranctionMessages = []
            };

            await _db.playerCollection.InsertOneAsync(player);
        }

        public async Task<bool> HasProfileAsync(SocketGuildUser user) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            return await _db.playerCollection.Find(filter).AnyAsync();
        }

        public async Task<PlayerModel> GetUserAsync(SocketGuildUser user) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            return await _db.playerCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(SocketGuildUser user, PlayerModel player) {
            var filter = Builders<PlayerModel>.Filter.Eq(x => x.DiscordID, user.Id);
            await _db.playerCollection.ReplaceOneAsync(filter, player);
        }
    }
}
