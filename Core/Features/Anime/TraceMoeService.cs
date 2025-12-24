using Discord;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Models.API;
using Lyuze.Core.Shared.Embeds;
using Lyuze.Core.Shared.Images;

namespace Lyuze.Core.Features.Anime {
    public class TraceMoeService(ILoggingService logger, IApiClient apiClient, EmbedService embedService) {
        private readonly ILoggingService _logger = logger;
        private readonly IApiClient _api = apiClient;
        private readonly EmbedService _embedService = embedService;


        public async Task<Embed> GetAnimeFromImageUrlAsync(string imageUrl, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(imageUrl)) {
                await _logger.LogInformationAsync("tracemoe", "Empty image URL received.");
                return await _embedService.ErrorEmbedAsync("TraceMoe", "Empty image URL received.");
            }
            var encodedImageUrl = Uri.EscapeDataString(imageUrl.Trim());
            var url = $"https://api.trace.moe/search?url={encodedImageUrl}";
            var model = await _api.GetJsonAsync("tracemoe", url, TraceMoe.FromJson,ct);
            var result = model?.Result;
            if (result == null || result.Length == 0) {
                await _logger.LogWarningAsync("tracemoe", "No results found.");
                return await _embedService.ErrorEmbedAsync("TraceMoe", "No results in found");
            }
            var topResult = result[0];

            var embed = new EmbedBuilder()
                .WithTitle("Anime Search Result")
                .WithDescription($"**Anime ID:** {topResult.AnilistId}\n" +
                                 $"**Filename:** {topResult.Filename}\n" +
                                 $"**Episode:** {(topResult.Episode.HasValue ? topResult.Episode.Value.ToString() : "N/A")}\n" +
                                 $"**From:** {TimeSpan.FromSeconds(topResult.From):hh\\:mm\\:ss}\n" +
                                 $"**To:** {TimeSpan.FromSeconds(topResult.To):hh\\:mm\\:ss}\n" +
                                 $"**Similarity:** {topResult.Similarity:P2}")
                .WithImageUrl(topResult.Image ?? string.Empty)
                .WithUrl(topResult.Video ?? string.Empty)
                .WithColor(await ColorUtils.RandomColorFromUrlAsync(topResult.Image ?? ImageConfig.BackupImageUrl))
                .Build();

            return embed;
        }

    }
}
