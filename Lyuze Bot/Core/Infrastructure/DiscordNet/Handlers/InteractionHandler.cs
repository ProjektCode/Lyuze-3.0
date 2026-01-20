using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using System.Reflection;

namespace Lyuze.Core.Infrastructure.DiscordNet.Handlers {
    public class InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services, ILoggingService logger) {
        private readonly ILoggingService _logger = logger;

        public async Task InitAsync() {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg) {
            try {
                var ctx = new SocketInteractionContext(client, arg);
                await commands.ExecuteCommandAsync(ctx, services);
            } catch (Exception ex) {
                await _logger.LogErrorAsync("interact", "Error handling interaction", ex);
            }
        }
    }
}
