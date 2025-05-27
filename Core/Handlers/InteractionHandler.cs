using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lyuze.Core.Handlers {
    public class InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services) {
        public async Task InitAsync() {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg) {
            try {
                var ctx = new SocketInteractionContext(client, arg);
                await commands.ExecuteCommandAsync(ctx, services);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
