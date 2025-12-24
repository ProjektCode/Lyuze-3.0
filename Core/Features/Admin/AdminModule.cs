using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Extensions;
using System.Diagnostics;

namespace Lyuze.Core.Features.Admin {
    public class AdminModule() : InteractionModuleBase<SocketInteractionContext> {

        [SlashCommand("purge", "Purge messages from the last 14 days")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeCmd(int amount = 200) {
            await DeferAsync(ephemeral: true);

            if (amount <= 0) {
                await FollowupAsync("Amount must be greater than 0.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            if (amount > 1000) {
                await FollowupAsync("Amount must be 1000 or less.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            try {
                var channel = (ITextChannel)Context.Channel;
                var cutoff = DateTimeOffset.UtcNow.AddDays(-14);

                var toDelete = new List<IMessage>(capacity: Math.Min(amount, 1000));
                ulong? beforeId = null;

                while (toDelete.Count < amount) {
                    var remaining = amount - toDelete.Count;
                    var pageSize = Math.Min(100, remaining);

                    IEnumerable<IMessage> page;

                    if (beforeId.HasValue) {
                        page = await Context.Channel
                            .GetMessagesAsync(beforeId.Value, Direction.Before, pageSize)
                            .FlattenAsync();
                    } else {
                        page = await Context.Channel
                            .GetMessagesAsync(pageSize)
                            .FlattenAsync();
                    }

                    var pageList = page.ToList();
                    if (pageList.Count == 0)
                        break;

                    foreach (var msg in pageList) {
                        if (msg.Timestamp < cutoff) {
                            beforeId = null;
                            break;
                        }

                        if (msg is IUserMessage userMsg) {
                            toDelete.Add(userMsg);
                        }
                    }

                    beforeId = pageList.Last().Id;

                    if (pageList.Last().Timestamp < cutoff)
                        break;
                }

                if (toDelete.Count == 0) {
                    await FollowupAsync("Nothing to delete (within 14 days).", ephemeral: true);
                    await Context.DelayDeleteOriginalAsync();
                    return;
                }

                // Bulk delete expects a collection
                await channel.DeleteMessagesAsync(toDelete);

                await FollowupAsync($"Deleted {toDelete.Count} message(s) from the last 14 days.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex);
                await FollowupAsync("An error occurred trying to purge messages.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }
        }


        [SlashCommand("kick", "Kick user from the server")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickCmd(SocketGuildUser user, string reason = "No reason given.") {
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
        public async Task BanCmd(SocketGuildUser user,int pruneDays ,string reason = "No reason given.") {
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

        //[SlashCommand("test", "test command")]
        //[RequireOwner]
        //public async Task TestCommand() {

        //}
    }
}
