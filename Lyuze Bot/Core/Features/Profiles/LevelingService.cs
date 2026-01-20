using System.Collections.Concurrent;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Shared.Images;

namespace Lyuze.Core.Features.Profiles {
    public class LevelingService(IPlayerService player, ColorUtils colorUtils) {

        private readonly IPlayerService _player = player;
        private readonly ColorUtils _colorUtils = colorUtils;

        // Thread-safe dictionary for tracking message cooldowns
        // Key = User ID, Value = Timestamp of their last XP-awarding message
        private readonly ConcurrentDictionary<ulong, DateTimeOffset> _messageCooldowns = new();
        
        private const int CooldownSeconds = 3;
        private const int CleanupThresholdSeconds = 30;

        public static double LevelEquation(int lvl) {
            double xp = Math.Floor(Math.Round(25 * Math.Pow(lvl + 1, 2)));
            return xp;
        }

        public async Task<bool> CanLevelUpAsync(SocketGuildUser user) {
            PlayerModel player = await _player.GetUserAsync(user);
            double needXP = LevelEquation(player.Level);

            return player.XP >= needXP;
        }

        public async Task LevelUpAsync(SocketGuildUser user, SocketCommandContext ctx) {
            PlayerModel player = await _player.GetUserAsync(user);

            int xp = 0;
            double neededXp = LevelEquation(player.Level);

            if (player.XP >= neededXp) {
                xp = (int)(player.XP - neededXp);
            }

            player.Level++;
            player.XP = xp;

            if (player.LevelNotify) {
                var avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

                EmbedBuilder embed = new() {
                    Title = $"{user.Username} has reached level {player.Level}!",
                    Description = $"{LevelEquation(player.Level)} XP needed for the next level.",
                    Color = new Color(await _colorUtils.RandomColorFromUrlAsync(avatarUrl)),
                    ThumbnailUrl = avatarUrl
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build());
            }

            await _player.UpdateUserAsync(user, player);
        }

        public async Task GiveXPAsync(SocketGuildUser user, int xp = 1) {
            PlayerModel player = await _player.GetUserAsync(user);
            player.XP += xp;
            await _player.UpdateUserAsync(user, player);
        }

        public async Task MsgCoolDownAsync(IUserMessage message, SocketCommandContext ctx, int xp = 1) {
            var userId = message.Author.Id;
            var now = DateTimeOffset.Now;

            // Lazy cleanup - remove stale entries older than threshold
            CleanupStaleCooldowns(now);

            // Check if user is in cooldown
            if (_messageCooldowns.TryGetValue(userId, out var lastMessageTime)) {
                if (lastMessageTime.AddSeconds(CooldownSeconds) <= now) {
                    // Cooldown expired - update timestamp and give XP
                    _messageCooldowns[userId] = now;
                    await LevelHelperAsync((SocketGuildUser)message.Author, xp, ctx);
                }
                // else: still in cooldown, no XP awarded
            } else {
                // First message from this user - add to tracking and give XP
                _messageCooldowns[userId] = now;
                await LevelHelperAsync((SocketGuildUser)message.Author, xp, ctx);
            }
        }

        /// <summary>
        /// Removes stale cooldown entries to prevent unbounded memory growth.
        /// </summary>
        private void CleanupStaleCooldowns(DateTimeOffset now) {
            var cutoff = now.AddSeconds(-CleanupThresholdSeconds);
            foreach (var kvp in _messageCooldowns) {
                if (kvp.Value < cutoff) {
                    _messageCooldowns.TryRemove(kvp.Key, out _);
                }
            }
        }

        private async Task LevelHelperAsync(SocketGuildUser user, int xp, SocketCommandContext ctx) {
            await GiveXPAsync(user, xp);
            if (await CanLevelUpAsync(user)) {
                await LevelUpAsync(user, ctx);
            }
        }
    }
}
