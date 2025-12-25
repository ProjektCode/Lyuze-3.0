using Discord;
using Discord.Interactions;
using Lyuze.Core.Extensions;

namespace Lyuze.Core.Features.Anime {
    public class AnimeModule(AnimeQuoteService animeQuoteService, TraceMoeService traceMoeService, SauceNaoService sauceNaoService, WaifuService waifuService) : InteractionModuleBase<SocketInteractionContext> {
        private readonly AnimeQuoteService _animeQuoteService = animeQuoteService;
        private readonly TraceMoeService _traceMoeService = traceMoeService;
        private readonly SauceNaoService _sauceNaoService = sauceNaoService;
        private readonly WaifuService _waifuService = waifuService;

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

        [SlashCommand("trace", "Find the anime from an image (URL or upload)")]
        public async Task TraceMoe([Summary("image", "Upload an image")] IAttachment? image = null, [Summary("url", "Direct image URL")] string? url = null) {
                await DeferAsync();

                // Require one or the other
                if (image is null && string.IsNullOrWhiteSpace(url)) {
                    await FollowupAsync("Provide an image attachment or a URL.");
                    return;
                }

                // If attachment provided, validate it's an image
                if (image is not null) {
                    if (!IsImageAttachment(image)) {
                        await FollowupAsync("That attachment doesn't look like an image. Please upload a PNG/JPG/GIF/WebP.");
                        return;
                    }

                    url = image.Url; // Discord-hosted CDN URL
                }

                // At this point url is guaranteed
                var embed = await _traceMoeService.GetAnimeFromImageUrlAsync(url!);

                await FollowupAsync(embed: embed);
            }

        [SlashCommand("sauce", "Find the source of an image (URL or upload)")]
        public async Task SauceNao([Summary("image", "Upload an image")] IAttachment? image = null, [Summary("url", "Direct image URL")] string? url = null) {
            await DeferAsync();

            // Require one or the other
            if (image is null && string.IsNullOrWhiteSpace(url)) {
                await FollowupAsync("Provide an image attachment or a URL.");
                return;
            }

            string finalUrl;

            // If attachment provided, validate it's an image
            if (image is not null) {
                if (!IsImageAttachment(image)) {
                    await FollowupAsync("That attachment doesn't look like an image. Please upload a PNG/JPG/GIF/WebP.");
                    return;
                }

                finalUrl = image.Url; // non-null
            } else {
                finalUrl = url!; // safe due to earlier guard
            }

            var result = await _sauceNaoService.GetSauceFromImageUrlAsync(finalUrl);


            if (result.FileBytes is not null && result.FileName is not null) {
                using var ms = new MemoryStream(result.FileBytes);
                await FollowupWithFileAsync(ms, result.FileName, embed: result.Embed);
            } else {
                await FollowupAsync(embed: result.Embed);
            }

        }

        [SlashCommand("waifu", "Get a random waifu image")]
        public async Task WaifuCmd([Summary("tag", "The tag of the waifu image: https://www.waifu.im/tags/")] string tag) {

            await DeferAsync();

            var imageUrl = await _waifuService.GetRandomWaifuPicAsync(tag);

            if (imageUrl is null) {
                await FollowupAsync("I couldn't find a waifu image for that tag or no image was found.");
                return;
            }

            await FollowupAsync(imageUrl);
        }

        private static bool IsImageAttachment(IAttachment attachment) {
                // Best case: Discord gives us a proper mime type
                if (!string.IsNullOrWhiteSpace(attachment.ContentType)) {
                    if (attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                // Fallback: check extension
                var filename = attachment.Filename ?? "";
                return filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || filename.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                    || filename.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                    || filename.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
            }

        }
}
