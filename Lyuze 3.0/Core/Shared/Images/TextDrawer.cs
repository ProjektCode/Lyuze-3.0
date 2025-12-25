using System.Drawing;

namespace Lyuze.Core.Shared.Images {
    public static class TextDrawer {
        public static Image DrawTextToImage(Image img, string header, string subHeader) {
            using var g = Graphics.FromImage(img);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var headFont = new Font("Roboto", 30, FontStyle.Regular);
            using var subFont = new Font("Roboto", 23, FontStyle.Regular);
            using var brushRed = new SolidBrush(Color.Crimson);
            using var brushGray = new SolidBrush(ColorTranslator.FromHtml("#B3B3B3"));

            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.DrawString(header, headFont, brushRed, img.Width / 2f, img.Height / 2f + 140, format);
            g.DrawString(subHeader, subFont, brushGray, img.Width / 2f, img.Height / 2f + 185, format);

            return new Bitmap(img);
        }
    }

}
