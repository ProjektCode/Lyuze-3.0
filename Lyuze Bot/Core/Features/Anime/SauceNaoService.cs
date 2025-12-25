using Discord;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Models.API;
using Lyuze.Core.Shared.Embeds;

namespace Lyuze.Core.Features.Anime {
    public class SauceNaoService(ILoggingService logger, IApiClient apiClient, EmbedService embedService, SettingsConfig settingsConfig) {
        private readonly ILoggingService _logger = logger;
        private readonly IApiClient _api = apiClient;
        private readonly EmbedService _embedService = embedService;
        private readonly SettingsConfig _settings = settingsConfig;

        public sealed record EmbedWithFile(Embed Embed, byte[]? FileBytes, string? FileName);

        public async Task<EmbedWithFile> GetSauceFromImageUrlAsync(string imageUrl, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(imageUrl)) {
                return new(await _embedService.ErrorEmbedAsync("SauceNao", "Empty image URL received."), null, null);
            }

            var apiKey = _settings.ApIs?.SauceNao;
            if (string.IsNullOrWhiteSpace(apiKey)) {
                return new(await _embedService.ErrorEmbedAsync("SauceNao", "SauceNao API key is not configured."), null, null);
            }

            var db = "dbs[]=5&dbs[]=37&dbs[]=9&dbs[]=12&dbs[]=14&dbs[]=16&dbs[]=25&db=26&dbs[]=27&dbs[]=39&dbs[]=41";
            var url = $"https://saucenao.com/search.php?api_key={Uri.EscapeDataString(apiKey)}&{db}&output_type=2&numres=16&url={Uri.EscapeDataString(imageUrl.Trim())}";

            var model = await _api.GetJsonAsync("saucenao", url, SauceNao.FromJson, ct);
            var results = model?.Results;
            var header = model?.Header; 

            if (results is null || results.Length == 0) {
                return new(await _embedService.ErrorEmbedAsync("SauceNao", "No results found."), null, null);
            }

            var top = results[0];
            var topHeader = header;

            if (topHeader?.ShortRemaining < 1) {
                return new(await _embedService.ErrorEmbedAsync("SauceNao", "API request limit reached for the short period. Try again in 30 seconds."), null, null);
            }

            var links = results
                .Take(3)
                .SelectMany(r => r.Data?.ExtUrls ?? [])
                .Select(u => u.AbsoluteUri)
                .Distinct()
                .Take(3)
                .ToList();

            var desc = links.Count > 0
                ? "***Top 3 results:***\n" + string.Join("\n", links)
                : "No external links were provided.";

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Highest Result - {top.Data?.Creator ?? "Unknown"} | {top.Header.Similarity ?? "Unknown "}%")
                .WithDescription(desc)
                .WithUrl(links.FirstOrDefault() ?? string.Empty)
                .WithFooter(f => {
                    f.Text = "SauceNao Search - Click title for most relevant result";
                    f.IconUrl = "https://saucenao.com/favicon.ico";
                });

            byte[]? bytes = null;
            string? fileName = null;

            var thumbUrl = top.Header?.Thumbnail?.AbsoluteUri;
            if (!string.IsNullOrWhiteSpace(thumbUrl)) {
                try {
                    // download yourself (Discord won't have to)
                    using var http = new HttpClient();
                    bytes = await http.GetByteArrayAsync(thumbUrl, ct);
                    fileName = "saucenao-thumb.jpg";
                    embedBuilder.WithImageUrl($"attachment://{fileName}");
                } catch (Exception ex) {
                    await _logger.LogWarningAsync("saucenao", $"Thumbnail download failed (will send embed without image): {ex.Message}");
                }
            }

            // pick a safe color source (don't fetch ext urls for colors)
            embedBuilder.WithColor(Color.DarkRed);

            return new(embedBuilder.Build(), bytes, fileName);
        }

    }
}
