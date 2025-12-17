using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Services.Extensions;
using Lyuze.Core.Services.Images;
using Lyuze.Core.Utilities;
using System.Diagnostics;

namespace Lyuze.Core.Modules {
    public class AdminModule : InteractionModuleBase<SocketInteractionContext> {

        [SlashCommand("purge", "Purge messages from the last 14 days")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeCmd(int amount = 1000) {
            await DeferAsync(ephemeral: true);

            try {
                if (amount <= 0) {
                    await FollowupAsync("Amount of messages to remove must be greater than 0");
                    await Context.DelayDeleteOriginalAsync();
                    return;
                }
                if (amount > 1000) {
                    await FollowupAsync("Amount must be 1000 or less.");
                    await Context.DelayDeleteOriginalAsync();
                    return;
                }

                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14).ToList();
                var count = filteredMessages.Count;

                if (count == 0) {
                    await FollowupAsync("Nothing to delete.", ephemeral: true);
                    await Context.DelayDeleteOriginalAsync();
                } else {
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);

                    await FollowupAsync($"I've deleted {count} {(count > 1 ? "messages" : "message")}.", ephemeral: true);
                    await Context.DelayDeleteOriginalAsync();
                }

            } catch (Exception ex) {
                Console.Write(ex.ToString());
                await FollowupAsync("An error occured trying to purge the messages.");
                await Context.DelayDeleteOriginalAsync();
            }

        }

        [SlashCommand("kick", "Kick user from the server")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickCmd(SocketGuildUser user, String reason = "No reason given.") {
            try {
                await user.KickAsync(reason);
                await RespondAsync($"User {user.Username} has been kicked for: {reason} - by {Context.User.Username}.");
                await Context.DelayDeleteOriginalAsync();

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await RespondAsync("An error has occured trying to kick user", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }
        }

        [SlashCommand("ban", "Ban user from the server")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanCmd(SocketGuildUser user,int pruneDays ,String reason = "No reason given.") {
            try {
                await Context.Guild.AddBanAsync(user,pruneDays, reason);
                await RespondAsync($"User {user.Username} has been banned for: {reason} - by {Context.User.Username}.");
                await Context.DelayDeleteOriginalAsync();

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await RespondAsync("An error has occured trying to ban user", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
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
                    await Context.DelayDeleteOriginalAsync();
                    p.Kill();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                await FollowupAsync("An error has occurred while trying to kill the bot.");
                await Context.DelayDeleteOriginalAsync();
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
