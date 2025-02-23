using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace Lyuze.Core.Handlers {
    public class InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services) {

        private readonly DiscordSocketClient _client = client;
        private readonly InteractionService _commands = commands;
        private readonly IServiceProvider _services = services;

        public async Task InitAsync() {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg) {
            try {
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);

            } catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
            }

        }

    }
}
