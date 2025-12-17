using Discord;
using Discord.Interactions;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Lyuze.Core.Configuration;
using Lyuze.Core.Database;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Handlers;
using Lyuze.Core.Services;
using Lyuze.Core.Services.Database;
using Lyuze.Core.Services.Interfaces;
using Lyuze.Core.Services.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lyuze {
    public class Program {

        public static Task Main() => MainAsync();

        public static async Task MainAsync() {
            // Load settings once and initialize DB before building the host
            //await SettingsConfig.LoadAsync();
            //var settings = SettingsConfig.Instance;

            //var dbContext = new DatabaseContext(settings);
            //Player.Initialize(dbContext);

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => {
                    // Core config & logging
                    services.AddSingleton<ILoggingService, LoggingService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IPlayerService, PlayerService>();

                    services.AddSingleton(settings => settings.GetRequiredService<ISettingsService>().Value); // SettingsConfig

                    // DatabaseContext depends on SettingsConfig
                    services.AddSingleton<DatabaseContext>(settings => new DatabaseContext(settings.GetRequiredService<SettingsConfig>()));

                    // Discord client
                    services.AddSingleton(sp => new DiscordSocketClient(new DiscordSocketConfig {
                        GatewayIntents = GatewayIntents.All,
                        WebSocketProvider = WS4NetProvider.Instance,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        DefaultRetryMode = RetryMode.AlwaysRetry,
                        LogGatewayIntentWarnings = false,
                        MessageCacheSize = 102
                    }));

                    // Interaction service
                    services.AddSingleton(sp =>
                        new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

                    // Handlers
                    services.AddSingleton<InteractionHandler>();
                    services.AddSingleton<Core.Handlers.EventHandler>();

                    // Bot services
                    services.AddSingleton<ReactionRolesService>();
                    services.AddSingleton<LevelingService>();
                    services.AddSingleton<N8nService>();

                    //Providers
                    services.AddSingleton<IStatusProvider, StatusProvider>();
                    services.AddSingleton<IEmbedColorProvider, EmbedColorProvider>();

                    // Typed HttpClient for WaifuService
                    services.AddHttpClient<WaifuService>();

                    // Host logging configuration
                    services.AddLogging(logging => {
                        logging.ClearProviders();
                        logging.AddSimpleConsole();
                        logging.SetMinimumLevel(LogLevel.Information);

                        logging.AddFilter("System.Net.Http.HttpClient.WaifuService.LogicalHandler", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.WaifuService.ClientHandler", LogLevel.Warning);
                        logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);
                        logging.AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning);
                        logging.AddFilter("Lyuze", LogLevel.Debug);
                    });
                })
                .Build();

            await RunAsync(host);
        }

        public static async Task RunAsync(IHost host) {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var interactionService = serviceProvider.GetRequiredService<InteractionService>();
            var settings = serviceProvider.GetRequiredService<SettingsConfig>();
            var player = serviceProvider.GetRequiredService<IPlayerService>();

            // Initialize interaction handler (register slash commands, etc.)
            await serviceProvider.GetRequiredService<InteractionHandler>().InitAsync();

            var dbContext = serviceProvider.GetRequiredService<DatabaseContext>();

            // Force construction of event handler so it wires events
            serviceProvider.GetRequiredService<Core.Handlers.EventHandler>();

            await client.LoginAsync(TokenType.Bot, settings.Discord.Token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
