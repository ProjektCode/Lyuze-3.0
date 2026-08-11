using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using System.Reflection;

namespace Lyuze.Core.Infrastructure.DiscordNet.Handlers {
    public class InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services, ILoggingService logger) {
        private readonly ILoggingService _logger = logger;

        public async Task InitAsync() {
            var result =await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg) {
            try {
                var ctx = new SocketInteractionContext(client, arg);
               var result = await commands.ExecuteCommandAsync(ctx, services);

               if (!result.IsSuccess) await _logger.LogCriticalAsync("interact", $"Failed to execute command - reason: {result.Error}");

            } catch (Exception ex) {
                await _logger.LogErrorAsync("interact", "Error handling interaction", ex);
            }
        }
    }
}
