using Discord.Interactions;
using Lyuze.Core.Features.Utility.Services;
using Lyuze.Core.Shared.Components;

namespace Lyuze.Core.Features.Utility.Components {
    public class HelpMenuHandler( HelpService helpService): InteractionModuleBase<SocketInteractionContext> {
        private readonly HelpService _helpService = helpService;

        [ComponentInteraction(CustomIds.HelpCategoryPattern)]
        public async Task HandleCategorySelectAsync(string invokerUserIDText, string[] selectedValues) {


            //Check user ID
            if (!CustomIds.TryParseUserId(invokerUserIDText, out ulong invokerUserID) || invokerUserID != Context.User.Id) {
                await RespondAsync("You cannot use this menu.", ephemeral: true);
                return;
            }

            //Read selected category
            var categoryKey = selectedValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(categoryKey)) {
                await RespondAsync("You must select a valid category.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            //Ask help service for commands
            var commands = await _helpService.GetVisibleCommandsForCategoryAsync(Context, categoryKey);

            if(commands == null || !commands.Any()) {
                await ModifyOriginalResponseAsync(msg => {
                    msg.Content = "No commands found for this category.";
                    msg.Embed = null;
                    msg.Components = null;
                });
                return;
            }

            //Update message
            var (embed, component) = await _helpService.RenderCommandListAsync(Context, commands);
            await ModifyOriginalResponseAsync(msg => {
                msg.Embed = embed;
                msg.Components = component;
            });
        }
    }
}
