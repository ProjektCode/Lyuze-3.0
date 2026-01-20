using Discord;
using Discord.Interactions;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Features.Admin;
using Lyuze.Core.Features.Anime;
using Lyuze.Core.Features.Integrations.N8n;
using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Features.Roles;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Infrastructure.Database;
using Lyuze.Core.Infrastructure.DiscordNet;
using Lyuze.Core.Infrastructure.DiscordNet.Handlers;
using Lyuze.Core.Infrastructure.Http;
using Lyuze.Core.Infrastructure.Logging;
using Lyuze.Core.Shared.Embeds;
using Lyuze.Core.Shared.Embeds.Providers;
using Lyuze.Core.Shared.Images;
using Lyuze.Core.Shared.Images.Primitives;
using Lyuze.Core.Shared.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lyuze;

public class Program {

    public static async Task Main() {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => {
                // Core config & logging
                services.AddSingleton<ILoggingService, LoggingService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IPlayerService, PlayerService>();

                services.AddSingleton(sp => sp.GetRequiredService<ISettingsService>().Value); // SettingsConfig

                // DatabaseContext depends on SettingsConfig
                services.AddSingleton<DatabaseContext>(sp => new DatabaseContext(sp.GetRequiredService<SettingsConfig>()));

                // Discord client
                services.AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig {
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
                services.AddSingleton<AdminService>();
                services.AddSingleton<ReactionRolesService>();
                services.AddSingleton<LevelingService>();
                services.AddSingleton<N8nService>();
                services.AddSingleton<WaifuService>();
                services.AddSingleton<AnimeQuoteService>();
                services.AddSingleton<TraceMoeService>();
                services.AddSingleton<IEmbedService, EmbedService>();
                services.AddSingleton<SauceNaoService>();
                services.AddSingleton<ProfileService>();
                services.AddSingleton<ImageFetcher>();
                services.AddSingleton<ColorUtils>();

                // Providers
                services.AddSingleton<IStatusProvider, StatusProvider>();
                services.AddSingleton<IEmbedColorProvider, EmbedColorProvider>();

                // Http client services
                services.AddHttpClient<IApiClient, ApiClient>();

                // Hosted service for Discord bot lifecycle
                services.AddHostedService<DiscordStartupService>();

                // Host logging configuration
                services.AddLogging(logging => {
                    logging.ClearProviders();
                    logging.AddSimpleConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);

                    // Suppress Microsoft host lifetime logs (we use our own logging)
                    logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                    logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);

                    // Logging filters to remove noisy HTTP logs
                    logging.AddFilter("System.Net.Http.HttpClient.WaifuService.LogicalHandler", LogLevel.Warning);
                    logging.AddFilter("System.Net.Http.HttpClient.WaifuService.ClientHandler", LogLevel.Warning);
                    logging.AddFilter("System.Net.Http.HttpClient.AnimeQuoteService.LogicalHandler", LogLevel.Warning);
                    logging.AddFilter("System.Net.Http.HttpClient.AnimeQuoteService.ClientHandler", LogLevel.Warning);
                    logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);
                    logging.AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning);
                    logging.AddFilter("System.Net.Http.HttpClient.IApiClient.LogicalHandler", LogLevel.Warning);
                    logging.AddFilter("System.Net.Http.HttpClient.IApiClient.ClientHandler", LogLevel.Warning);

                    // Allow Lyuze namespace debug logs through ILogger (if any modules still use it)
                    logging.AddFilter("Lyuze", LogLevel.Debug);
                });
            })
            .Build();

        // Run the host - this will start the DiscordStartupService and keep the app running
        await host.RunAsync();
    }
}
