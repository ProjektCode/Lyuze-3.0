using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Models.API;

namespace Lyuze.Core.Features.Anime {
    public class AnimeQuoteService(ILoggingService logger, IApiClient apiClient) {
        private readonly ILoggingService _logger = logger;
        private readonly IApiClient _apiClient = apiClient;

        public async Task<string?> GetRandomAnimeQuoteAsync(CancellationToken ct = default) {
            var model = await _apiClient.GetJsonAsync("anime-quote", "https://api.animechan.io/v1/quotes/random", AnimeRandomQuote.FromJson, ct);

            var quote = model?.Data?.Content;
            var character = model?.Data?.Character?.Name;

            if (string.IsNullOrWhiteSpace(quote) || string.IsNullOrWhiteSpace(character)) {
                await _logger.LogWarningAsync("anime-quote", "No quote or character found in the response.");
                return null;
            }

            return $"\"{quote}\" - {character}";
        }
    }
}
