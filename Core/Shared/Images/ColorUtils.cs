using ColorThiefDotNet;
using Lyuze.Core.Shared.Images.Primitives;
using System.Drawing;

namespace Lyuze.Core.Shared.Images {

    public sealed class ColorUtils(ImageFetcher fetcher) {
        private readonly ImageFetcher _fetcher = fetcher;
        private static readonly Random _rng = new();

        private static IEnumerable<string> GenerateColors(Bitmap bitmap) {
            var thief = new ColorThief();
            var palette = thief.GetPalette(bitmap, 9);
            return palette.Select(x => x.Color.ToHexString());
        }

        public async Task<uint> RandomColorFromUrlAsync(string url, CancellationToken ct = default) {
            if (string.IsNullOrWhiteSpace(url)) return Discord.Color.Red;

            url = url.Trim();

            // Fetch image in-memory
            var img = await _fetcher.FetchImageAsync(url, ct);
            if (img is null)
                return Discord.Color.Red;

            // Ensure we dispose downloaded image
            using (img) {
                // ColorThief wants a Bitmap
                using var bmp = img as Bitmap ?? new Bitmap(img);

                var colors = GenerateColors(bmp).ToArray();
                if (colors.Length == 0) return Discord.Color.Red;

                var randomHex = colors[_rng.Next(colors.Length)];
                var hex = randomHex.TrimStart('#');

                return Convert.ToUInt32(hex, 16);
            }
        }
    }

}
