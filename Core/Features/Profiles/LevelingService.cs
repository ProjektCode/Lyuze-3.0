using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Shared.Images;

namespace Lyuze.Core.Features.Profiles {
    public class LevelingService(IPlayerService player) {

        private readonly IPlayerService _player = player;

        // Instance-level state instead of static
        private readonly List<Message> _msgList = [];
        private readonly List<ulong> _authorList = [];

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
                    Color = new Color(await ColorUtils.RandomColorFromUrlAsync(avatarUrl)),
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
            Message newMsg = new() {
                AuthorID = message.Author.Id,
                Timestamp = message.Timestamp,
            };

            // If we’ve seen this author recently
            if (_authorList.Contains(newMsg.AuthorID)) {
                // Find their previous message
                var authorMsg = _msgList.Find(x => x.AuthorID == newMsg.AuthorID);

                // If at least 3 seconds have passed → award XP again
                if (authorMsg?.Timestamp.AddSeconds(3) <= DateTimeOffset.Now) {
                    _authorList.Remove(message.Author.Id);
                    if (authorMsg != null) {
                        _msgList.Remove(authorMsg);
                    }

                    await LevelHelperAsync((SocketGuildUser)message.Author, xp, ctx);
                }
            } else {
                // First message from this author (for our purposes)
                _authorList.Add(message.Author.Id);
                _msgList.Add(newMsg);
                await LevelHelperAsync((SocketGuildUser)message.Author, xp, ctx);
            }
        }

        private async Task LevelHelperAsync(SocketGuildUser user, int xp, SocketCommandContext ctx) {
            await GiveXPAsync(user, xp);
            if (await CanLevelUpAsync(user)) {
                await LevelUpAsync(user, ctx);
            }
        }
    }

    public class Message {
        public ulong AuthorID { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
