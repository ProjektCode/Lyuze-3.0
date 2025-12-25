using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Features.Anime {
    public class AnimeQuoteService(ILoggingService logger, HttpClient httpClient, IApiClient apiClient) {
        private readonly ILoggingService _logger = logger;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiClient _apiClient = apiClient;

        public async Task<string?> GetRandomAnimeQuoteAsync(CancellationToken ct = default) {
            //try {
            //    var response = await _httpClient.GetAsync("https://api.animechan.io/v1/quotes/random");
            //    if (!response.IsSuccessStatusCode) {
            //        await _logger.LogWarningAsync("anime-quote", $"Failed to fetch anime quote. HTTP {(int)response.StatusCode}");
            //        return null;
            //    }
            //    var content = await response.Content.ReadAsStringAsync();
            //    var quoteResponse = AnimeRandomQuote.FromJson(content);
            //    if (quoteResponse == null || string.IsNullOrWhiteSpace(quoteResponse?.Data?.Content)) {
            //        await _logger.LogWarningAsync("anime-quote", "No quote found in the response.");
            //        return null;
            //    }
            //    return $"\"{quoteResponse?.Data?.Content}\" - {quoteResponse?.Data.Character?.Name}";
            //} catch (Exception ex) {
            //    await _logger.LogErrorAsync("anime-quote", "Unexpected exception in AnimeQuoteService.GetRandomAnimeQuoteAsync.", ex);
            //    return null;
            //}

            var model = await _apiClient.GetJsonAsync("anime-quote", "https://api.animechan.io/v1/quotes/random", AnimeRandomQuote.FromJson, ct);

            var quote = model?.Data?.Content;
            var character = model?.Data?.Character?.Name;

            if(string.IsNullOrWhiteSpace(quote) || string.IsNullOrWhiteSpace(character)) {
                await _logger.LogWarningAsync("anime-quote", "No quote or character found in the response.");
                return null;
            }

            return $"\"{quote}\" - {character}";

        }

    }
}
