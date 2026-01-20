using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Extensions;
using Lyuze.Core.Shared.Images;
using Lyuze.Core.Shared.Images.Primitives;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Lyuze.Core.Features.Admin {
    public class AdminModule(
        IPlayerService playerService,
        ImageFetcher imageFetcher,
        ColorUtils colorUtils,
        AdminService adminService,
        ILogger<AdminModule> logger) : InteractionModuleBase<SocketInteractionContext> {
        
        private readonly IPlayerService _playerService = playerService;
        private readonly ImageFetcher _imageFetcher = imageFetcher;
        private readonly ColorUtils _colorUtils = colorUtils;
        private readonly AdminService _adminService = adminService;
        private readonly ILogger<AdminModule> _logger = logger;

        [SlashCommand("purge", "Purge messages from the last 14 days")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeCmd(int amount = 200) {
            await DeferAsync(ephemeral: true);

            try {
                var channel = (ITextChannel)Context.Channel;
                var result = await _adminService.GetMessagesToDeleteAsync(channel, amount);

                if (!result.IsSuccess) {
                    await FollowupAsync(result.ErrorMessage!, ephemeral: true);
                    await Context.DelayDeleteOriginalAsync();
                    return;
                }

                var deletedCount = await _adminService.DeleteMessagesAsync(channel, result.Messages!);

                await FollowupAsync($"Deleted {deletedCount} message(s) from the last 14 days.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error purging messages");
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
                _logger.LogError(ex, "Error kicking user {Username}", user.Username);
                await RespondAsync("An error has occured trying to kick user", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }
        }

        [SlashCommand("ban", "Ban user from the server")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanCmd(SocketGuildUser user, int pruneDays, string reason = "No reason given.") {
            try {
                await Context.Guild.AddBanAsync(user, pruneDays, reason);
                await RespondAsync($"User {user.Username} has been banned for: {reason} - by {Context.User.Username}.");
                await Context.DelayDeleteOriginalAsync();

            } catch (Exception ex) {
                _logger.LogError(ex, "Error banning user {Username}", user.Username);
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
                _logger.LogError(ex, "Error killing bot process");
                await FollowupAsync("An error has occurred while trying to kill the bot.");
                await Context.DelayDeleteOriginalAsync();
            }

        }

        [SlashCommand("test", "test command")]
        [RequireOwner]
        public async Task TestCommand() {

            await DeferAsync();

            if (Context.User is not SocketGuildUser user) {
                await FollowupAsync("This command must be used in a guild.");
                return;
            }

            // Ensure profile exists
            var player = await _playerService.GetUserAsync(user);
            if (player is null) {
                await FollowupAsync("No player profile found.");
                return;
            }

            // Generate image
            var imageBytes = await ImageGenerator.CreateWelcomeBannerAsync(
                user,
                player.Background,
                $"Welcome, {user.Username}!",
                "Image test successful",
                _imageFetcher,
                _colorUtils
            );

            // Send image as attachment
            await using var ms = new MemoryStream(imageBytes);
            await FollowupWithFileAsync(
                text: "Image generated successfully!",
                attachment: new FileAttachment(ms, "welcome-test.png")
            );

        }
    }
}
