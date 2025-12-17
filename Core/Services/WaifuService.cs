using Lyuze.Core.Services.Interfaces;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Services {

    public class WaifuService(ILoggingService logger, HttpClient httpClient) {
        private readonly ILoggingService _logger = logger;
        private readonly HttpClient _httpClient = httpClient;
        private static readonly Random _rng = new();

        public async Task<string?> GetRandomWaifuPicAsync(string tag) {
            if (string.IsNullOrWhiteSpace(tag)) {
                await _logger.LogInformationAsync("waifu", "Empty tag received.");
                return null;
            }

            var encodedTag = Uri.EscapeDataString(tag.Trim());

            try {
                var response = await _httpClient.GetAsync($"https://api.waifu.im/search?included_tags={encodedTag}&limit=20");

                if (!response.IsSuccessStatusCode) {
                    await _logger.LogWarningAsync("waifu",$"Failed to fetch waifu image. HTTP {(int)response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var waifuResponse = Waifu.FromJson(content);

                if (waifuResponse.Images == null || waifuResponse.Images.Length == 0) {
                    await _logger.LogWarningAsync("waifu", "No images found in the response.");
                    return null;
                }

                var index = _rng.Next(waifuResponse.Images.Length);
                var selectedImage = waifuResponse.Images[index];

                if (selectedImage.Url == null) {
                    await _logger.LogWarningAsync("waifu", "Selected image URL is null.");
                    return null;
                }

                return selectedImage.Url.ToString();
            } catch (Exception ex) {
                await _logger.LogErrorAsync("waifu", "Unexpected exception in WaifuService.GetRandomWaifuPicAsync.", ex);
                return null;
            }
        }
    }
}
