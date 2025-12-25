using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Lyuze.Core.Shared.Images;
using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Shared.Embeds {
    public class EmbedService(IEmbedColorProvider embedColorService, IPlayerService playerService, ILoggingService loggingService) {
        private readonly IPlayerService _playerService = playerService;
        private readonly IEmbedColorProvider _embedColorService = embedColorService;
        private readonly ILoggingService _loggingService = loggingService;

        // Private helper to create embed builders easily
        private EmbedBuilder CreateEmbed(string title, string description, Color? color) {
            var builder = new EmbedBuilder()
                .WithCurrentTimestamp();

            if (!string.IsNullOrWhiteSpace(title))
                builder.WithTitle(title);

            if (!string.IsNullOrWhiteSpace(description))
                builder.WithDescription(description);

            builder.WithColor(color ?? new Color(_embedColorService.GetRandomEmbedColor()));

            return builder;
        }

        public Task<Embed> ErrorEmbedAsync(string source, string error) {
            var embed = CreateEmbed($"ERROR | {source}", error, Color.Red).Build();
            return Task.FromResult(embed);
        }

        public async Task<Embed> ProfileEmbedAsync(SocketGuildUser user, SocketInteractionContext ctx) {

            try {

                PlayerModel player = await _playerService.GetUserAsync(user);
                Double requiredXp = LevelingService.LevelEquation(player.Level);

                var embed = new EmbedBuilder {
                    Title = $"{user.Username}'s Profile | Level - {player.Level} ({player.XP}/{requiredXp})",
                    ImageUrl = player.Background,
                    Color = new Color(await ColorUtils.RandomColorFromUrlAsync(player.Background)),
                    ThumbnailUrl = user.GetAvatarUrl(ImageFormat.Auto, 256) ?? user.GetDefaultAvatarUrl(),
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new EmbedFooterBuilder {
                        Text = "Profile Data",
                        IconUrl = ctx.Guild.IconUrl ?? user.GetDefaultAvatarUrl()
                    }
                };

                if (!string.IsNullOrEmpty(user.Nickname)) {
                    embed.AddField("Nickname", user.Nickname, inline: true);
                }

                embed.AddField("Account Creation",
                    user.CreatedAt.DateTime.ToShortDateString() ?? "N/A", inline: true);

                embed.AddField("Joined Server",
                    user.JoinedAt?.DateTime.ToShortDateString() ?? "N/A", inline: true);

                // Current activity: show first activity name or "N/A"
                var activity = user.Activities?.FirstOrDefault();
                var activityText = activity != null ? activity.Name : "N/A";
                embed.AddField("Current Activity", activityText, inline: true);

                embed.AddField("About Me", player.AboutMe ?? "N/A");

                return embed.Build();

            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("EmbedService", $"Error creating profile embed for {user.Username}: ", ex);
                return await ErrorEmbedAsync("GetProfileEmbed", ex.Message);
            }
        }

        public async Task<Embed> UpdatedProfileAsync(SocketGuildUser user, SocketInteractionContext ctx, String UpdatedSection, String entry) {
            try {

                var embed = new EmbedBuilder {
                    Title = "Profile Updated",
                    Description = $"{user.Username}'s {UpdatedSection} has been updated with: {entry}.",
                    Color = new Color(await ColorUtils.RandomColorFromUrlAsync(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())),
                    ThumbnailUrl = user.GetAvatarUrl(ImageFormat.Auto, 256) ?? user.GetDefaultAvatarUrl(),
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new EmbedFooterBuilder {
                        Text = "Profile Update",
                        IconUrl = ctx.Guild.IconUrl ?? user.GetDefaultAvatarUrl()
                    }
                };
                return embed.Build();

            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("EmbedService", $"Error creating profile update embed for {user.Username}: ", ex);
                return await ErrorEmbedAsync("ProfileUpdateEmbed", ex.Message);
            }

        }

    }
}
