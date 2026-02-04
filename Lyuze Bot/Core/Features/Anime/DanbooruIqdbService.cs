using System.Globalization;
using Discord;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Features.Anime;

public sealed class DanbooruIqdbService(ILoggingService logger, IApiClient apiClient, IEmbedService embedService, SettingsConfig settingsConfig) {
    private const int MaxCandidates = 5;
    private readonly ILoggingService _logger = logger;
    private readonly IApiClient _api = apiClient;
    private readonly IEmbedService _embedService = embedService;
    private readonly SettingsConfig _settings = settingsConfig;

    public async Task<EmbedWithFile> GetIqdbFromImageUrlAsync(string imageUrl, string? note = null, CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(imageUrl)) {
            return new(await _embedService.ErrorEmbedAsync("Danbooru IQDB", "Empty image URL received."), null, null);
        }

        var encoded = Uri.EscapeDataString(imageUrl.Trim());
        var url = $"https://danbooru.donmai.us/iqdb_queries.json?search[url]={encoded}";

        // Optional auth for better rate limits
        var login = _settings.ApIs?.DanbooruLogin?.Trim();
        var apiKey = _settings.ApIs?.DanbooruApiKey?.Trim();
        if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(apiKey)) {
            // Match Danbooru docs examples; also trimming avoids hidden whitespace causing "invalid api key".
            url += $"&api_key={Uri.EscapeDataString(apiKey)}&login={Uri.EscapeDataString(login)}";
        }

        var matches = await _api.GetJsonAsync("danbooru-iqdb", url, DanbooruIqdbMatch.FromJson, ct);
        if (matches is null || matches.Length == 0) {
            return new(await _embedService.ErrorEmbedAsync("Danbooru IQDB", "No matches found."), null, null);
        }

        var ordered = matches
            .OrderByDescending(m => m.Score)
            .Take(MaxCandidates)
            .ToList();

        var top = ordered[0];
        var postUrl = $"https://danbooru.donmai.us/posts/{top.PostId}";

        var descParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(note)) {
            descParts.Add(note);
            descParts.Add(string.Empty);
        }

        var artist = top.Post?.TagStringArtist;
        var source = top.Post?.Source;

        if (!string.IsNullOrWhiteSpace(artist)) {
            descParts.Add($"**Artist tags:** {Truncate(artist, 120)}");
        }
        if (!string.IsNullOrWhiteSpace(source)) {
            descParts.Add($"**Source:** {Truncate(source, 200)}");
        }

        descParts.Add(string.Empty);
        descParts.Add("**Top matches:**");
        foreach (var m in ordered) {
            descParts.Add($"- {m.Score.ToString("0.##", CultureInfo.InvariantCulture)}% https://danbooru.donmai.us/posts/{m.PostId}");
        }

        var image = MakeAbsoluteDanbooruUrl(top.Post?.LargeFileUrl)
                    ?? MakeAbsoluteDanbooruUrl(top.Post?.FileUrl)
                    ?? MakeAbsoluteDanbooruUrl(top.Post?.PreviewFileUrl);

        var embed = new EmbedBuilder()
            .WithTitle($"Danbooru IQDB - {top.Score.ToString("0.##", CultureInfo.InvariantCulture)}%")
            .WithUrl(postUrl)
            .WithDescription(string.Join("\n", descParts.Where(s => s is not null)))
            .WithImageUrl(image ?? string.Empty)
            .WithColor(Color.DarkOrange)
            .WithFooter(f => {
                f.Text = "Danbooru IQDB";
                f.IconUrl = "https://danbooru.donmai.us/favicon.ico";
            })
            .Build();

        return new(embed, null, null);
    }

    private static string? MakeAbsoluteDanbooruUrl(string? maybeRelative) {
        if (string.IsNullOrWhiteSpace(maybeRelative)) {
            return null;
        }

        if (Uri.TryCreate(maybeRelative, UriKind.Absolute, out var abs)) {
            return abs.AbsoluteUri;
        }

        if (maybeRelative.StartsWith('/')) {
            return "https://danbooru.donmai.us" + maybeRelative;
        }

        return null;
    }

    private static string Truncate(string input, int maxLen) {
        if (input.Length <= maxLen) {
            return input;
        }

        return input[..Math.Max(0, maxLen - 3)] + "...";
    }
}
