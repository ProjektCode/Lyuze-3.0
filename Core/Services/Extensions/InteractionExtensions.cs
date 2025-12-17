using Discord;
using Discord.Interactions;

namespace Lyuze.Core.Services.Extensions;

public static class InteractionExtensions {
    public static async Task DelayDeleteOriginalAsync(this IInteractionContext ctx, int seconds = 5) {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        await ctx.Interaction.DeleteOriginalResponseAsync();
    }
}
