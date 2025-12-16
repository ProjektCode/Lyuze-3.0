using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Utilities;
using Lyuze.Core.Services.Images;
using Lyuze.Core.Services.Database;

namespace Lyuze.Core.Services {
    public class EmbedService {
        private readonly MasterUtilities _utils;
        private readonly LevelingService _levelingService;

        public EmbedService(MasterUtilities utils, LevelingService levelingService) {
            _levelingService = levelingService;
            _utils = utils;
        }

        // Private helper to create embed builders easily
        private EmbedBuilder CreateEmbed(string title, string description, Color? color) {
            var builder = new EmbedBuilder()
                .WithCurrentTimestamp();

            if (!string.IsNullOrWhiteSpace(title))
                builder.WithTitle(title);

            if (!string.IsNullOrWhiteSpace(description))
                builder.WithDescription(description);

            builder.WithColor(color ?? new Color(_utils.RandomEmbedColor()));

            return builder;
        }

        public Task<Embed> ErrorEmbedAsync(string source, string error) {
            var embed = CreateEmbed($"ERROR | {source}", error, Color.Red).Build();
            return Task.FromResult(embed);
        }

        public async Task<Embed> ProfileEmbedAsync(
            SocketGuildUser user,
            SocketCommandContext ctx,
            PlayerModel player) {

            var embed = new EmbedBuilder {
                Title = $"{user.Username}'s Profile | Level - {player.Level}",
                ImageUrl = player.Background,
                Color = new Color(await ColorUtils.RandomColorFromUrlAsync(player.Background)),
                ThumbnailUrl = user.GetAvatarUrl(ImageFormat.Auto, 256) ?? user.GetDefaultAvatarUrl(),
                Timestamp = ctx.Message.Timestamp,
                Footer = new EmbedFooterBuilder {
                    Text = "Profile Data",
                    IconUrl = ctx.Guild.IconUrl
                }
            };

            if (!string.IsNullOrEmpty(user.Nickname)) {
                embed.AddField("Nickname", user.Nickname, inline: true);
            }

            // ✅ Use instance LevelingService
            var requiredXp = LevelingService.LevelEquation(player.Level);
            embed.AddField("XP", $"{player.XP}/{requiredXp}", inline: true);

            embed.AddField("Account Creation",
                user.CreatedAt.DateTime.ToShortDateString(), inline: true);

            embed.AddField("Joined Server",
                user.JoinedAt?.DateTime.ToShortDateString() ?? "N/A", inline: true);

            // Current activity: show first activity name or "N/A"
            var activity = user.Activities?.FirstOrDefault();
            var activityText = activity != null ? activity.Name : "N/A";
            embed.AddField("Current Activity", activityText, inline: true);

            embed.AddField("About Me", player.AboutMe ?? "N/A");

            return embed.Build();
        }
    }
}
