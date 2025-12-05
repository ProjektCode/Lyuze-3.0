using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Discord.Net.Providers.WS4Net;
using Lyuze.Core.Handlers;
using Lyuze.Core.Services;
using Lyuze.Core.Database;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Configuration;
using Lyuze.Core.Services.Database;

namespace Lyuze {
    public class Program {

        public static Task Main() => MainAsync();

        public static async Task MainAsync() {

            await SettingsConfig.LoadAsync();
            var settings = SettingsConfig.Instance;

            var DbCtx = new DatabaseContext(settings);
            Player.Initialize(DbCtx);

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services
                    .AddSingleton<LoggingService, LoggingService>()
                    .AddSingleton(_ => SettingsConfig.Instance)
                    .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig {
                        GatewayIntents = GatewayIntents.All,
                        WebSocketProvider = WS4NetProvider.Instance,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        DefaultRetryMode = RetryMode.AlwaysRetry,
                        LogGatewayIntentWarnings = false,
                        MessageCacheSize = 102 }))

                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    .AddSingleton<InteractionHandler>()
                    .AddSingleton<Core.Handlers.EventHandler>()
                    .AddSingleton<ReactionRolesService>()
                    .AddSingleton<LevelingService>()
                    .AddSingleton<N8nService>()
                    .AddLogging(x => { x.ClearProviders(); x.AddSimpleConsole(); x.SetMinimumLevel(LogLevel.Trace); x.AddFilter("Victoria.LavaNode", LogLevel.Information);})
                ).Build();

            await RunAsync(host);
        }

        public static async Task RunAsync(IHost host) {

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var _cmds = serviceProvider.GetRequiredService<InteractionService>();
            var _settings = serviceProvider.GetRequiredService<SettingsConfig>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitAsync();
            serviceProvider.GetRequiredService<Core.Handlers.EventHandler>();

            await _client.LoginAsync(TokenType.Bot, _settings.Discord.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

    }
}
