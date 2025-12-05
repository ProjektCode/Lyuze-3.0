using Lyuze.Core.Configuration;
using System.Drawing;

namespace Lyuze.Core.Services.Images {
    public static class ImageFetcher {
        public static async Task<Image> FetchImageAsync(string url) {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) {
                var backupResponse = await client.GetAsync(ImageSettings.BackupImageUrl);
                return Image.FromStream(await backupResponse.Content.ReadAsStreamAsync());
            }

            return Image.FromStream(await response.Content.ReadAsStreamAsync());
        }

        public static async Task<Bitmap> URLToBitmapAsync(string url) {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to download image. Status: {response.StatusCode}");

            await using var stream = await response.Content.ReadAsStreamAsync();
            return new Bitmap(stream);
        }

    }

}
