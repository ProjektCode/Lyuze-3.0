using Discord;
using Discord.WebSocket;
using Lyuze.Core.Configuration;
using System.Drawing;

namespace Lyuze.Core.Services.Images {
    public class ImageGenerator {
        public static async Task<string> CreateBannerImageAsync(SocketGuildUser user, string msg, string submsg) {
            var avatar = await ImageFetcher.FetchImageAsync(user.GetAvatarUrl(ImageFormat.Png, 2048) ?? user.GetDefaultAvatarUrl());
            var background = await ImageFetcher.FetchImageAsync(ImageConfig.DefaultBackgroundUrl);

            background = ImageUtils.CropToBanner(background);
            var border = ImageUtils.CircleBorder(avatar);
            avatar = ImageUtils.ClipToCircle(avatar);

            if (border is Bitmap bmap) {
                bmap.MakeTransparent();
            }

            var banner = ImageUtils.CopyRegionIntoBanner(border, avatar, background);
            banner = TextDrawer.DrawTextToImage(banner, msg, submsg);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.png");
            banner.Save(path);
            banner.Dispose();

            return path;
        }

        public static async Task<string> CreateImageAsync(int width, int height, string? url = null) {
            var background = await ImageFetcher.FetchImageAsync(url ?? ImageConfig.DefaultBackgroundUrl);

            if (background.Width < width || background.Height < height) {
                background = await ImageFetcher.FetchImageAsync(ImageConfig.DefaultBackgroundUrl);
                background = ImageUtils.CropToBanner(background);
                background = TextDrawer.DrawTextToImage(background, "Error has occurred.", "Invalid dimensions.");
            } else {
                background = ImageUtils.CropToBanner(background, width, height);
            }

            var path = $"{Guid.NewGuid()}.png";
            background.Save(path);
            background.Dispose();

            return path;
        }
    }

}
