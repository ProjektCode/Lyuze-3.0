using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Discord.Net.Providers.WS4Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lyuze.Core.Handlers;
using Microsoft.Extensions.Logging;
using Lyuze.Core.Services;

namespace Lyuze {
    public class Program {

        public static Task Main() => MainAsync();

        public static async Task MainAsync() {

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services
                    .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig {
                        GatewayIntents = GatewayIntents.All,
                        WebSocketProvider = WS4NetProvider.Instance,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        DefaultRetryMode = RetryMode.AlwaysRetry,
                        LogGatewayIntentWarnings = false,
                        MessageCacheSize = 1024
                    }
                ))
                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    .AddSingleton<InteractionHandler>()
                    .AddSingleton<Core.Handlers.EventHandler>()
                    .AddSingleton<ReactionRolesService>()
                    .AddLogging(x => { x.ClearProviders(); x.AddSimpleConsole(); x.SetMinimumLevel(LogLevel.Trace);})
                ).Build();

            await RunAsync(host);
        }

        public static async Task RunAsync(IHost host) {

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var _cmds = serviceProvider.GetRequiredService<InteractionService>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitAsync();
            serviceProvider.GetRequiredService<Core.Handlers.EventHandler>();

            await _client.LoginAsync(TokenType.Bot, SettingsHandler.Instance.Discord.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
