using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Utilities;
using Lyuze.Core.Database.Services;
using Lyuze.Core.Services.Images;

namespace Lyuze.Core.Handlers {
    public sealed class EmbedHandler {
        private readonly MasterUtilities _utils;

        // Inject dependencies via constructor (better for testing and flexibility)
        public EmbedHandler(MasterUtilities utils) {
            _utils = utils ?? throw new ArgumentNullException(nameof(utils));
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

        public static async Task<Embed> ProfileEmbedAsync(SocketGuildUser user, SocketCommandContext ctx, PlayerModel player) {
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

            if (!string.IsNullOrEmpty(user.Nickname)) embed.AddField("Nickname", user.Nickname, inline: true);

            embed.AddField("XP", $"{player.XP}/{LevelingService.LevelEquation(player.Level)}", inline: true);

            embed.AddField("Account Creation", user.CreatedAt.DateTime.ToShortDateString(), inline: true);

            embed.AddField("Joined Server", user.JoinedAt?.DateTime.ToShortDateString() ?? "N/A", inline: true);

            embed.AddField("Current Activity", user.Activities, inline: true);

            embed.AddField("About Me", player.AboutMe ?? "N/A");

            return embed.Build();
        }

        public Task<Embed> VictoriaNoQueueEmbedAsync() {
            var embed = CreateEmbed("NO QUEUE", "There are no songs in the current queue", Color.Red).Build();
            return Task.FromResult(embed);
        }

        public Task<Embed> VictoriaInvalidUsageEmbedAsync(ITextChannel channel) {
            var embed = CreateEmbed("Invalid Command Usage", $"Command cannot be used in {channel.Name}.", Color.Red).Build();
            return Task.FromResult(embed);
        }
    }
}
