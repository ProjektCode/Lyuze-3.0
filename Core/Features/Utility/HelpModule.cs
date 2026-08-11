using Discord.Interactions;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Features.Utility.Services;

namespace Lyuze.Core.Features.Utility {
    public class HelpModule(HelpService helpService, ILoggingService loggingService) : InteractionModuleBase<SocketInteractionContext> {
        private readonly HelpService _helpService = helpService;

        [SlashCommand("help", "Displays help information about available commands.")]
        public async Task HelpAsync() {
            await DeferAsync(ephemeral: true);

            var (embed, component) = await _helpService.RenderCategoryPickerAsync(Context, Context.User.Id);

            await FollowupAsync(embed: embed, components: component, ephemeral: true);
        }
    }
}
