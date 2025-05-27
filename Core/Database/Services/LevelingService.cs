using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyuze.Core.Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Lyuze.Core.Database.Services {
    public class LevelingService() {

        private static readonly List<Message> MsgList = [];
        private static readonly List<ulong> AuthorList = [];

        public static Double LevelEquation(int lvl) {
            Double xp = Math.Floor(Math.Round(25 * Math.Pow(lvl + 1, 2)));
            return xp;
        }

        public async Task<bool> CanLevelUp(SocketGuildUser user) {
            PlayerModel _player = await Player.GetUserAsync(user);
            Double needXP = LevelEquation(_player.Level);

            if (_player.XP >= needXP) {
                return true;
            }
            return false;
        }

        public async Task LevelUp(SocketGuildUser user, SocketCommandContext ctx) {
            PlayerModel player = await Player.GetUserAsync(user);

            int xp = 0;
            if (player.XP >= LevelEquation(player.Level)) xp = (int)(player.XP - LevelEquation(player.Level));

            player.Level++;
            player.XP = xp;

            if (player.LevelNotify) {
                EmbedBuilder embed = new() { 
                        Title = $"{user.Username} Has reached level {player.Level}!",
                        Description = $"{LevelEquation(player.Level)} xp needed for the next level.",
                        Color = Color.Red, //Change this when we add the images utility class
                        ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build());
            }

            await Player.UpdateUserAsync(user, player);
        }

        public async Task GiveXP(SocketGuildUser user, int xp = 1) {
            PlayerModel player = await Player.GetUserAsync(user);
            player.XP += xp;
            await Player.UpdateUserAsync(user, player);
        }

        public async Task MsgCoolDownAsync(IUserMessage message, SocketCommandContext ctx, int xp = 1) { 
            Message newMsg = new() { 
                AuthorID = message.Author.Id,
                Timestamp = message.Timestamp,
            };

            if (AuthorList.Contains(newMsg.AuthorID)) {
                //Chek current time and see if 3 seconds have passed since last message
               var AuthorMsg = MsgList.Find(x => x.AuthorID == newMsg.AuthorID);
                if(AuthorMsg?.Timestamp.AddSeconds(3) !>= DateTimeOffset.Now) {
                    AuthorList.Remove(message.Author.Id);
                    MsgList.Remove(AuthorMsg);
                    await LevelHelper((SocketGuildUser)message.Author, xp, ctx);
                }
            } else {
                AuthorList.Add(message.Author.Id);
                MsgList.Add(newMsg);
                await LevelHelper((SocketGuildUser)message.Author, xp, ctx);
            }
        }

        public async Task LevelHelper(SocketGuildUser user, int xp, SocketCommandContext ctx) {
            await GiveXP(user, xp);
            if(await CanLevelUp(user)) await LevelUp(user, ctx);
        }
    }

    public class Message {
        public ulong AuthorID { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
