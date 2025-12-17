using Lyuze.Core.Services.Interfaces;

namespace Lyuze.Core.Services.Providers {
    /// <summary>
    /// Returns a random color as a uint, based on predefined hex codes.
    /// </summary>
    public class EmbedColorProvider : IEmbedColorProvider {

        private static readonly string[] colors = [
                "DC143C", // Crimson
                "C3E4E8", // Light Cyan
                "FF5733", // Light Orange
                "E6E6FA", // Lavender
                "7289DA", // Discord Purple
                "5865F2", // Discord Blurple
                "D2042D", // Cherry Red
                "8DB600", // Apple Green
                "87CEEB"  // Sky Blue
            ];

        public uint GetRandomEmbedColor() {

            var hex = colors[Random.Shared.Next(colors.Length)];

            // Parse as HEX, not decimal
            return Convert.ToUInt32(hex, 16);
        }

    }
}
