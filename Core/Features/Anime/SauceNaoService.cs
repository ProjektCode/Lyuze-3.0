using System.Globalization;
using Discord;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Features.Anime {
    public class SauceNaoService(ILoggingService logger, IApiClient apiClient, IEmbedService embedService, SettingsConfig settingsConfig, DanbooruIqdbService danbooruIqdbService) {
        private const double MinSimilarityPercent = 70.0;
        private const int MaxCandidates = 5;
        private readonly ILoggingService _logger = logger;
        private readonly IApiClient _api = apiClient;
        private readonly IEmbedService _embedService = embedService;
        private readonly SettingsConfig _settings = settingsConfig;

        private readonly DanbooruIqdbService _danbooruIqdb = danbooruIqdbService;

        public async Task<EmbedWithFile> GetSauceFromImageUrlAsync(string imageUrl, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(imageUrl)) {
                return new(await _embedService.ErrorEmbedAsync("SauceNao", "Empty image URL received."), null, null);
            }

            var apiKey = _settings.ApIs?.SauceNao?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey)) {
                // SauceNAO not configured - use Danbooru IQDB fallback
                return await _danbooruIqdb.GetIqdbFromImageUrlAsync(imageUrl, "SauceNAO API key not configured. Using Danbooru IQDB instead.", ct);
            }

            var db = "dbs[]=5&dbs[]=9&dbs[]=12&dbs[]=25&dbs[]=26&dbs[]=37&dbs[]=39&dbs[]=41&dbs[]=43&";
            var url = $"https://saucenao.com/search.php?api_key={Uri.EscapeDataString(apiKey)}&{db}&output_type=2&numres=32&url={Uri.EscapeDataString(imageUrl.Trim())}";

            var model = await _api.GetJsonAsync("saucenao", url, SauceNao.FromJson, ct);
            var results = model?.Results;
            var header = model?.Header; 

            if (results is null || results.Length == 0) {
                // No SauceNAO results - try Danbooru IQDB
                return await _danbooruIqdb.GetIqdbFromImageUrlAsync(imageUrl, "SauceNAO returned no results. Trying Danbooru IQDB.", ct);
            }

            var top = results[0];
            var topHeader = header;

            if (topHeader?.ShortRemaining < 1) {
                // SauceNAO rate limited - try Danbooru IQDB
                return await _danbooruIqdb.GetIqdbFromImageUrlAsync(imageUrl, "SauceNAO rate limit hit. Trying Danbooru IQDB.", ct);
            }

            var topSimilarity = ParseSimilarityPercent(top.Header?.Similarity);
            if (topSimilarity < MinSimilarityPercent) {
                return await _danbooruIqdb.GetIqdbFromImageUrlAsync(
                    imageUrl,
                    $"SauceNAO top match was only {topSimilarity.ToString("0.##", CultureInfo.InvariantCulture)}% (below {MinSimilarityPercent.ToString("0.##", CultureInfo.InvariantCulture)}%). Trying Danbooru IQDB.",
                    ct
                );
            }

            var candidates = results
                .Select(r => new {
                    Result = r,
                    Similarity = ParseSimilarityPercent(r.Header?.Similarity)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(MaxCandidates)
                .ToList();

            var topLinks = candidates
                .SelectMany(c => c.Result.Data?.ExtUrls ?? [])
                .Select(u => u.AbsoluteUri)
                .Distinct()
                .Take(3)
                .ToList();

            var lines = new List<string> {
                "**Top candidates:**"
            };
            foreach (var c in candidates) {
                var link = c.Result.Data?.ExtUrls?.FirstOrDefault()?.AbsoluteUri;
                if (!string.IsNullOrWhiteSpace(link)) {
                    lines.Add($"- {c.Similarity.ToString("0.##", CultureInfo.InvariantCulture)}% {link}");
                }
            }

            var desc = topLinks.Count > 0
                ? string.Join("\n", lines)
                : "No external links were provided.";

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"SauceNAO - {top.Data?.Creator ?? top.Data?.Author ?? top.Data?.Artist ?? "Unknown"} | {topSimilarity.ToString("0.##", CultureInfo.InvariantCulture)}%")
                .WithDescription(desc)
                .WithUrl(topLinks.FirstOrDefault() ?? string.Empty)
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
                    bytes = await _api.GetBytesAsync("saucenao", thumbUrl, ct);
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

        private static double ParseSimilarityPercent(string? similarity) {
            if (string.IsNullOrWhiteSpace(similarity)) {
                return 0;
            }

            // SauceNAO returns strings like "87.65".
            return double.TryParse(similarity, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

    }
}
