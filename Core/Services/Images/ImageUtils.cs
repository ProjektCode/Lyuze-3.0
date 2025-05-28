using System.Drawing.Drawing2D;
using System.Drawing;

namespace Lyuze.Core.Services.Images {
    public static class ImageUtils {
        public static Bitmap CropToBanner(Image img, int width = 1100, int height = 450) {
            var destinationSize = new Size(width, height);
            var ratio = Math.Min((float)img.Width / width, (float)img.Height / height);
            var srcRect = new Rectangle(
                (int)((img.Width - (width * ratio)) / 2),
                (int)((img.Height - (height * ratio)) / 2),
                (int)(width * ratio),
                (int)(height * ratio)
            );

            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, new Rectangle(Point.Empty, destinationSize), srcRect, GraphicsUnit.Pixel);
            return bmp;
        }

        public static Image CircleBorder(Image img) {
            var dest = new Bitmap(img.Width, img.Height, img.PixelFormat);
            using var g = Graphics.FromImage(dest);
            var r = new Rectangle(0, 0, img.Width, img.Height);
            using var pen = new Pen(Color.Crimson, img.Width / 2f);
            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawEllipse(pen, r);
            return dest;
        }

        public static Image ClipToCircle(Image img) {
            var dest = new Bitmap(img.Width, img.Height, img.PixelFormat);
            using var g = Graphics.FromImage(dest);
            g.Clear(Color.Transparent);
            var r = new Rectangle(0, 0, img.Width, img.Height);
            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(img, Point.Empty);
            return dest;
        }

        public static Image CopyRegionIntoBanner(Image border, Image avatar, Image background) {
            using var g = Graphics.FromImage(background);
            int x = (background.Width - 225) / 2;
            int y = (background.Height - 225) / 2;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(border, x - 4, y - 4, 233, 233);
            g.DrawImage(avatar, x, y, 225, 225);

            return background;
        }

        public static void DeleteImageFile(string path) {
            try {

                if (File.Exists(path)) {
                    File.Delete(path);
                }

            } catch (Exception ex) {
                Console.WriteLine($"[ImageUtils] Failed to delete image file: {path}. Exception: {ex.Message}");
            }

        }

    }
}
