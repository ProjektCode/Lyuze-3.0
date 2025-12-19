using Discord.Interactions;
using Lyuze.Core.Services.Extensions;
using Lyuze.Core.Services;

namespace Lyuze.Core.Modules {
    public class AnimeModule(AnimeQuoteService animeQuoteService): InteractionModuleBase<SocketInteractionContext> {
        private readonly AnimeQuoteService _animeQuoteService = animeQuoteService;

        [SlashCommand("aquote", "Get a random anime quote")]
        public async Task AnimeQuoteCmd() {
            await DeferAsync();
            var quote = await _animeQuoteService.GetRandomAnimeQuoteAsync();
            if (quote is null) {
                await FollowupAsync("I couldn't find an anime quote right now.");
                return;
            }
            await ReplyAsync(quote);
            await Context.DelayDeleteOriginalAsync();
        }

    }
}
