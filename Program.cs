using Discord;
using Discord.Interactions;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Features.Anime;
using Lyuze.Core.Features.Integrations.N8n;
using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Features.Roles;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Infrastructure.Database;
using Lyuze.Core.Infrastructure.DiscordNet.Handlers;
using Lyuze.Core.Infrastructure.Http;
using Lyuze.Core.Infrastructure.Logging;
using Lyuze.Core.Shared.Embeds;
using Lyuze.Core.Shared.Embeds.Providers;
using Lyuze.Core.Shared.Status;
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
                    services.AddSingleton<Core.Infrastructure.DiscordNet.Handlers.EventHandler>();

                    // Bot services
                    services.AddSingleton<ReactionRolesService>();
                    services.AddSingleton<LevelingService>();
                    services.AddSingleton<N8nService>();
                    services.AddSingleton<WaifuService>();
                    services.AddSingleton<AnimeQuoteService>();
                    services.AddSingleton<TraceMoeService>();
                    services.AddSingleton<EmbedService>();
                    services.AddSingleton<SauceNaoService>();

                    //Providers
                    services.AddSingleton<IStatusProvider, StatusProvider>();
                    services.AddSingleton<IEmbedColorProvider, EmbedColorProvider>();

                    // Http client services
                    services.AddHttpClient<IApiClient, ApiClient>();

                    // Host logging configuration
                    services.AddLogging(logging => {
                        logging.ClearProviders();
                        logging.AddSimpleConsole();
                        logging.SetMinimumLevel(LogLevel.Information);

                        //Logging filters to remove noisy logs
                        logging.AddFilter("System.Net.Http.HttpClient.WaifuService.LogicalHandler", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.WaifuService.ClientHandler", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.AnimeQuoteService.LogicalHandler", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.AnimeQuoteService.ClientHandler", LogLevel.Warning);
                        logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);
                        logging.AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.IApiClient.LogicalHandler", LogLevel.Warning);
                        logging.AddFilter("System.Net.Http.HttpClient.IApiClient.ClientHandler", LogLevel.Warning);

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
            serviceProvider.GetRequiredService<Core.Infrastructure.DiscordNet.Handlers.EventHandler>();

            await client.LoginAsync(TokenType.Bot, settings.Discord.Token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
