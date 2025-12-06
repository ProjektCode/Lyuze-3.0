using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Services;

namespace Lyuze.Core.Modules {

    public class WaifuModule(WaifuService waifuService) : InteractionModuleBase<SocketInteractionContext> {
        private readonly WaifuService _waifuService = waifuService;

        [SlashCommand("waifu", "Get a random waifu image")]
        public async Task WaifuCmd([Summary("tag", "The tag of the waifu image: https://www.waifu.im/tags/")] string tag) {

            await DeferAsync();

            var imageUrl = await _waifuService.GetRandomWaifuPicAsync(tag);

            if (imageUrl is null) {
                await FollowupAsync("I couldn't find a waifu image for that tag.");
                return;
            }

            await FollowupAsync(imageUrl);
        }
    }
}