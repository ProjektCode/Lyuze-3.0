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
        ILogger<AdminModule> logger,
        IEmbedService embedService) : InteractionModuleBase<SocketInteractionContext> {
        
        private readonly IPlayerService _playerService = playerService;
        private readonly ImageFetcher _imageFetcher = imageFetcher;
        private readonly ColorUtils _colorUtils = colorUtils;
        private readonly AdminService _adminService = adminService;
        private readonly ILogger<AdminModule> _logger = logger;
        private readonly IEmbedService _embedService = embedService;

        [SlashCommand("purge", "Purge messages from the last 14 days")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeCmd(int amount = 200) {
            await DeferAsync(ephemeral: true);

            try {
                var channel = (ITextChannel)Context.Channel;
                var result = await AdminService.GetMessagesToDeleteAsync(channel, amount);

                if (!result.IsSuccess) {
                    await FollowupAsync(result.ErrorMessage!, ephemeral: true);
                    await Context.DelayDeleteOriginalAsync();
                    return;
                }

                var deletedCount = await AdminService.DeleteMessagesAsync(channel, result.Messages!);

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
            await DeferAsync(ephemeral: true);

            var result = await AdminService.BanUserAsync(Context.Guild, user, pruneDays, reason);

            if (!result.IsSuccess) {
                await FollowupAsync(embed: await _embedService.ErrorEmbedAsync("Admin", error: result.ErrorMessage ?? "Unknown Error Occurred."), ephemeral: true);
                _logger.LogError("Error banning user {Username}: {ErrorMessage}", user.Username, result.ErrorMessage);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            await FollowupAsync(
                $"Successfully banned **{result.Username}** for reason: {result.Reason} - by {Context.User.Username}.",
                ephemeral: true
            );
            await Context.DelayDeleteOriginalAsync(10);
        }

        [SlashCommand("unban", "Unban a user from the server")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanCmd() {
            await DeferAsync(ephemeral: true);

            // 1. Fetch banned users (max 25 for SelectMenu)
            var bans = await AdminService.GetBannedUsersAsync(Context.Guild, limit: 25);

            // 2. Handle empty ban list
            if (bans.Count == 0) {
                await FollowupAsync("No banned users found.", ephemeral: true);
                return;
            }

            // 3. Build SelectMenu options
            var options = bans.Select(ban => new SelectMenuOptionBuilder()
                .WithLabel(Truncate($"{ban.User.Username} ({ban.User.Id})", 100))
                .WithValue(ban.User.Id.ToString())
                .WithDescription(Truncate($"Reason: {ban.Reason ?? "No reason provided"}", 100))
            ).ToList();

            // 4. Build the SelectMenu component
            // Embed moderator ID in customId for security verification
            var menu = new SelectMenuBuilder()
                .WithCustomId($"unban-select:{Context.User.Id}")
                .WithPlaceholder("Select a user to unban")
                .WithMinValues(1)
                .WithMaxValues(1)
                .WithOptions(options);

            var component = new ComponentBuilder()
                .WithSelectMenu(menu)
                .Build();

            // 5. Build the embed
            var description = bans.Count >= 25
                ? "Showing first 25 banned users.\nFor others, use `/unban-id <userId>`."
                : $"Found {bans.Count} banned user(s). Select one to unban.";

            var embed = new EmbedBuilder()
                .WithTitle("Unban User")
                .WithDescription(description)
                .WithColor(Color.Orange)
                .WithFooter("This dropdown expires after 15 minutes.")
                .Build();

            // 6. Send the response
            await FollowupAsync(embed: embed, components: component, ephemeral: true);
        }

        [SlashCommand("unban-id", "Unban a user by their Discord ID (for large ban lists)")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanIdCmd(
            [Summary("user-id", "The user's Discord ID (numeric)")] string userId,
            [Summary("reason", "Reason for unbanning (appears in audit log)")] string? reason = null) {

            await DeferAsync(ephemeral: true);

            var result = await AdminService.UnbanUserAsync(Context.Guild, userId, reason);

            if (!result.IsSuccess) {
                await FollowupAsync($"Failed to unban: {result.ErrorMessage}", ephemeral: true);
                return;
            }

            await FollowupAsync(
                $"Successfully unbanned **{result.Username}** (ID: {userId}).",
                ephemeral: true
            );
            await Context.DelayDeleteOriginalAsync(10);
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

        /// <summary>
        /// Truncates a string to a maximum length, adding "..." if truncated.
        /// </summary>
        private static string Truncate(string input, int maxLength) {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength) {
                return input ?? string.Empty;
            }
            return input[..(maxLength - 3)] + "...";
        }

    }
}
