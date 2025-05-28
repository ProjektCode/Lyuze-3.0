using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Services.Images;
using Lyuze.Core.Utilities;
using System.Diagnostics;

namespace Lyuze.Core.Modules {
    public class AdminModule : InteractionModuleBase<SocketInteractionContext> {

        [SlashCommand("purge", "Purge messages from the last 14 days")]
        [RequireBotPermission(Discord.GuildPermission.ManageMessages)]
        [RequireUserPermission(Discord.GuildPermission.ManageMessages)]
        public async Task PurgeCmd(int amount = 1000) {
            await DeferAsync(ephemeral: true);

            try {
                if (amount <= 0) {
                    await FollowupAsync("Amount of messages to remove must be greater than 0");
                    await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                    return;
                }
                if (amount > 1000) {
                    await FollowupAsync("Amount must be 1000 or less.");
                    await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                    return;
                }

                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14).ToList();
                var count = filteredMessages.Count;

                if (count == 0) {
                    await FollowupAsync("Nothing to delete.", ephemeral: true);
                    await MasterUtilities.DelayAndDeleteResponseAsync(Context); ;
                } else {
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);

                    await FollowupAsync($"I've deleted {count} {(count > 1 ? "messages" : "message")}.", ephemeral: true);
                    await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                }

            } catch (Exception ex) {
                Console.Write(ex.ToString());
                await FollowupAsync("An error occured trying to purge the messages.");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }

        }

        [SlashCommand("kick", "Kick user from the server")]
        [RequireBotPermission(Discord.GuildPermission.KickMembers)]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task KickCmd(SocketGuildUser user, String reason = "No reason given.") {
            try {
                await user.KickAsync(reason);
                await RespondAsync($"User {user.Username} has been kicked for: {reason} - by {Context.User.Username}.");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await RespondAsync("An error has occured trying to kick user", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }
        }

        [SlashCommand("ban", "Ban user from the server")]
        [RequireBotPermission(Discord.GuildPermission.BanMembers)]
        [RequireUserPermission(Discord.GuildPermission.BanMembers)]
        public async Task BanCmd(SocketGuildUser user,int pruneDays ,String reason = "No reason given.") {
            try {
                await Context.Guild.AddBanAsync(user,pruneDays, reason);
                await RespondAsync($"User {user.Username} has been banned for: {reason} - by {Context.User.Username}.");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await RespondAsync("An error has occured trying to ban user", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }
        }

        [SlashCommand("kill", "Kill the bot")]
        [RequireOwner]
        public async Task KillCmd() {
            await DeferAsync(ephemeral: true);

            try {
                var currentProcessName = Process.GetCurrentProcess().ProcessName;

                foreach (Process p in Process.GetProcessesByName(currentProcessName)) {
                    await FollowupAsync("Goodbye");
                    await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                    p.Kill();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                await FollowupAsync("An error has occurred while trying to kill the bot.");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }

        }

        [SlashCommand("test", "test command")]
        [RequireOwner]
        public async Task TestCommand() {
            await DeferAsync();

            var user = (SocketGuildUser)Context.User;

            string message = "Test main message";
            string submsg = "Test sub message";

            string imagePath = await ImageGenerator.CreateBannerImageAsync(user, message, submsg);
            await FollowupWithFileAsync(imagePath);
            ImageUtils.DeleteImageFile(imagePath);
            
        }
    }
}
