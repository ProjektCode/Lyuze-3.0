using System.Drawing;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Shared.Images.Primitives {

    public sealed class ImageFetcher(IApiClient apiClient, ILoggingService logger) {
        private readonly IApiClient _api = apiClient;
        private readonly ILoggingService _logger = logger;

        public async Task<byte[]?> FetchBytesAsync(string url, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return await _api.GetBytesAsync("images", url, ct);
        }

        public async Task<Image?> FetchImageAsync(string url, CancellationToken ct = default) {
            var bytes = await FetchBytesAsync(url, ct);
            if (bytes is null || bytes.Length == 0) {
                await _logger.LogWarningAsync("images", $"No bytes returned for {url}");
                return null;
            }

            // Image.FromStream requires stream to stay alive unless you clone.
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);

            // Clone into a standalone Bitmap so the stream can be disposed safely.
            return new Bitmap(img);
        }
    }

}
