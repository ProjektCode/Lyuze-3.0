using System.Drawing;

namespace Lyuze.Core.Services.Extensions {
    public static class ColorExtensions {
        public static string ToHexString(this Color color) {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

}
