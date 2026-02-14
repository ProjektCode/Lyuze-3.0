using System.Drawing;
using System.Drawing.Text;

namespace Lyuze.Core.Shared.Images.Primitives {

    public static class TextDrawer {

        public static void DrawCenteredTextBlock(
            Graphics g,
            string headline,
            string subText,
            RectangleF bounds,
            float headlineMaxFont,
            float headlineMinFont,
            float subtextMinFont
        ) {
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using var white = new SolidBrush(Color.White);

            // Fit headline font to width
            float headlineSize = headlineMaxFont;
            string finalHeadline = headline;

            Font headlineFont;
            SizeF headlineMeasured;

            do {
                headlineFont = new Font("Arial", headlineSize, FontStyle.Bold);
                headlineMeasured = g.MeasureString(finalHeadline, headlineFont);

                if (headlineMeasured.Width <= bounds.Width)
                    break;

                headlineFont.Dispose();
                headlineSize -= 1f;

            } while (headlineSize >= headlineMinFont);

            if (headlineMeasured.Width > bounds.Width) {
                finalHeadline = TrimToFit(g, finalHeadline, headlineFont, bounds.Width);
                headlineMeasured = g.MeasureString(finalHeadline, headlineFont);
            }

            // Subtext proportional to headline (but not too small)
            float subSize = Math.Max(subtextMinFont, headlineSize * 0.60f);
            using var subFont = new Font("Arial", subSize, FontStyle.Regular);
            var subMeasured = g.MeasureString(subText, subFont);

            // Center within bounds
            float totalH = headlineMeasured.Height + 6 + subMeasured.Height;
            float startY = bounds.Y + (bounds.Height - totalH) / 2f;

            float headlineX = bounds.X + (bounds.Width - headlineMeasured.Width) / 2f;
            float subX = bounds.X + (bounds.Width - subMeasured.Width) / 2f;

            g.DrawString(finalHeadline, headlineFont, white, headlineX, startY);
            g.DrawString(subText, subFont, white, subX, startY + headlineMeasured.Height + 6);

            headlineFont.Dispose();
        }

        private static string TrimToFit(Graphics g, string text, Font font, float maxWidth) {
            const string ellipsis = "…";

            for (int i = text.Length - 1; i > 0; i--) {
                var test = string.Concat(text.AsSpan(0, i), ellipsis);
                if (g.MeasureString(test, font).Width <= maxWidth)
                    return test;
            }

            return ellipsis;
        }
    }
}
