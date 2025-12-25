using ColorThiefDotNet;
using System.Drawing;

namespace Lyuze.Core.Shared.Images {
    public static class ColorUtils {
        public static IEnumerable<string> GenerateColors(Bitmap bitmap) {
            var thief = new ColorThief();
            var palette = thief.GetPalette(bitmap, 9);
            return palette.Select(x => x.Color.ToHexString());
        }

        public static async Task<uint> RandomColorFromUrlAsync(string url) {
            if (url.Contains(' '))
                url = url.Replace(" ", "-");

            var bitmap = await ImageFetcher.URLToBitmapAsync(url);
            var colors = GenerateColors(bitmap);
            var randomHex = colors.ElementAt(new Random().Next(colors.Count()));

            var hex = randomHex.Replace("#", "");
            return Convert.ToUInt32(hex, 16);
        }
    }

}
