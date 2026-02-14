using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Lyuze.Core.Shared.Images {

    public static class ImageUtils {
        public static Bitmap To32bppArgb(Image src) {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(src, 0, 0, src.Width, src.Height);
            return bmp;
        }

        /// <summary>
        /// Draws an image into a destination rectangle using "cover" semantics:
        /// preserves aspect ratio and crops excess, centered.
        /// </summary>
        public static void DrawImageCover(Graphics g, Image image, Rectangle destRect) {
            // Source size
            float srcW = image.Width;
            float srcH = image.Height;

            // Destination size
            float dstW = destRect.Width;
            float dstH = destRect.Height;

            // Scale to cover
            float scale = Math.Max(dstW / srcW, dstH / srcH);

            float scaledW = srcW * scale;
            float scaledH = srcH * scale;

            // Center crop: compute top-left of scaled image so it centers in dest
            float x = destRect.X + (dstW - scaledW) / 2f;
            float y = destRect.Y + (dstH - scaledH) / 2f;

            g.DrawImage(image, x, y, scaledW, scaledH);
        }

        /// <summary>
        /// Convenience helper: returns a cropped banner bitmap using "cover" semantics.
        /// </summary>
        public static Bitmap CropToBanner(Image src, int width, int height) {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            DrawImageCover(g, src, new Rectangle(0, 0, width, height));
            return bmp;
        }

        public static Bitmap ClipToCircle(Image image) {
            using var src = To32bppArgb(image);

            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(dst);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Transparent fill so we don't get black corners
            g.Clear(Color.Transparent);

            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, src.Width, src.Height);
            g.SetClip(path);

            g.DrawImage(src, 0, 0, src.Width, src.Height);
            g.ResetClip();

            return dst;
        }

        public static Bitmap AddCircleBorder(Image circularImage, Color borderColor, float borderThicknessPx) {
            using var src = To32bppArgb(circularImage);

            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(dst);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.Clear(Color.Transparent);
            g.DrawImage(src, 0, 0, src.Width, src.Height);

            var inset = borderThicknessPx / 2f;
            var rect = new RectangleF(inset, inset, src.Width - borderThicknessPx, src.Height - borderThicknessPx);

            using var pen = new Pen(borderColor, borderThicknessPx);
            g.DrawEllipse(pen, rect);

            return dst;
        }

        /// <summary>
        /// Fast blur approximation by scaling down and back up.
        /// scale: 0.03 - 0.12 are typical values. Smaller => blurrier.
        /// </summary>
        public static Bitmap BlurByDownscale(Image src, int width, int height, float scale = 0.06f) {
            int wSmall = Math.Max(1, (int)(width * scale));
            int hSmall = Math.Max(1, (int)(height * scale));

            using var small = new Bitmap(wSmall, hSmall, PixelFormat.Format32bppArgb);
            using (var gSmall = Graphics.FromImage(small)) {
                gSmall.InterpolationMode = InterpolationMode.HighQualityBilinear;
                gSmall.SmoothingMode = SmoothingMode.HighQuality;
                gSmall.DrawImage(src, 0, 0, wSmall, hSmall);
            }

            var blurred = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(blurred)) {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(small, 0, 0, width, height);
            }

            return blurred;
        }

    }

}
