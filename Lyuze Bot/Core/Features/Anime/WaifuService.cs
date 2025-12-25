using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Features.Anime {
    public class WaifuService(ILoggingService logger, IApiClient apiClient) {
        private readonly ILoggingService _logger = logger;
        private readonly IApiClient _api = apiClient;
        private static readonly Random _rng = new();

        public async Task<string?> GetRandomWaifuPicAsync(string tag, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(tag)) {
                await _logger.LogInformationAsync("waifu", "Empty tag received.");
                return null;
            }

            var encodedTag = Uri.EscapeDataString(tag.Trim());
            var url = $"https://api.waifu.im/search?included_tags={encodedTag}&limit=20";

            var model = await _api.GetJsonAsync(source: "waifu", url: url, deserialize: Waifu.FromJson, ct: ct);

            var images = model?.Images;
            if (images == null || images.Length == 0) {
                await _logger.LogWarningAsync("waifu", "No images found in the response.");
                return null;
            }

            var selected = images[_rng.Next(images.Length)];
            if (selected.Url == null) {
                await _logger.LogWarningAsync("waifu", "Selected image URL is null.");
                return null;
            }

            return selected.Url.ToString();
        }
    }
}
