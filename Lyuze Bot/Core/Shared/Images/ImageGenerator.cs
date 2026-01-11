using Discord.WebSocket;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Shared.Images.Primitives;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Lyuze.Core.Shared.Images {

    public static class  ImageGenerator {

        public static async Task<byte[]> CreateWelcomeBannerAsync(SocketGuildUser user, string backgroundUrl, string headline, string subText, ImageFetcher fetcher,
          ColorUtils colorUtils, int width = 1100, int height = 450, CancellationToken ct = default) {

            const int superSample = 2; // try 2 or 3

            int rw = width * superSample;
            int rh = height * superSample;

            var bg = (await fetcher.FetchImageAsync(backgroundUrl, ct)
                      ?? await fetcher.FetchImageAsync(ImageConfig.DefaultBackgroundUrl, ct))
                     ?? throw new InvalidOperationException("Failed to fetch background.");

            var avatarUrl = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl();
            var avatar = await fetcher.FetchImageAsync(avatarUrl, ct)
                         ?? throw new InvalidOperationException("Failed to fetch avatar.");

            using (bg)
            using (avatar)
            using (var renderCanvas = new Bitmap(rw, rh, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(renderCanvas)) {

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var layout = new BannerLayout(rw, rh);

                ImageUtils.DrawImageCover(g, bg, new Rectangle(0, 0, rw, rh));

                using (var overlay = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                    g.FillRectangle(overlay, 0, 0, rw, rh);

                using (var panelBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                    GraphicsUtils.FillRoundedRect(g, panelBrush, layout.PanelRect, layout.PanelCornerRadius);

                using (var panelPen = new Pen(Color.FromArgb(60, 255, 255, 255), Math.Max(1f, 2f * layout.Scale)))
                    GraphicsUtils.DrawRoundedRect(g, panelPen, layout.PanelRect, layout.PanelCornerRadius);

                uint rgb = await colorUtils.RandomColorFromUrlAsync(avatarUrl, ct);
                var borderColor = Color.FromArgb(
                    255,
                    (int)((rgb >> 16) & 0xFF),
                    (int)((rgb >> 8) & 0xFF),
                    (int)(rgb & 0xFF)
                );

                using var circle = ImageUtils.ClipToCircle(avatar);
                using var circleWithBorder = ImageUtils.AddCircleBorder(circle, borderColor, layout.AvatarBorderThickness);

                g.DrawImage(circleWithBorder, layout.AvatarTopLeft.X, layout.AvatarTopLeft.Y, layout.AvatarSize, layout.AvatarSize);

                var textBounds = new RectangleF(
                    layout.PanelRect.X,
                    layout.TextStart.Y,
                    layout.PanelRect.Width,
                    (layout.PanelRect.Bottom - layout.TextStart.Y) - (layout.PanelRect.Height * 0.08f)
                );

                TextDrawer.DrawCenteredTextBlock(
                    g,
                    headline,
                    subText,
                    textBounds,
                    layout.HeadlineMaxFont,
                    layout.HeadlineMinFont,
                    layout.SubtextMinFont
                );

                // Downscale to final output (this is the magic)
                using var finalCanvas = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (var g2 = Graphics.FromImage(finalCanvas)) {
                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g2.CompositingQuality = CompositingQuality.HighQuality;

                    g2.DrawImage(renderCanvas, new Rectangle(0, 0, width, height));
                }

                using var ms = new MemoryStream();
                finalCanvas.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

    }

}
