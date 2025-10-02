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
using Lyuze.Core.Database.Services;
using Victoria;
using System.Diagnostics;

namespace Lyuze {
    public class Program {

        public static Task Main() => MainAsync();

        public static async Task MainAsync() {
            RunLavalink();
            await SettingsHandler.LoadAsync();
            var settings = SettingsHandler.Instance;

            var DbCtx = new DatabaseContext(settings);
            Player.Initialize(DbCtx);

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services
                    .AddSingleton<ILoggingService, LoggingService>()
                    .AddSingleton(_ => SettingsHandler.Instance)
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
                    .AddLavaNode(x => {
                            x.SelfDeaf = false;
                            x.Hostname = "127.0.0.1";
                            x.Port = 2333;
                            x.Authorization = "youshallnotpass";
                        })
                    .AddSingleton<AudioService>()
                    .AddLogging(x => { x.ClearProviders(); x.AddSimpleConsole(); x.SetMinimumLevel(LogLevel.Trace); x.AddFilter("Victoria.LavaNode", LogLevel.Information);})
                ).Build();

            await RunAsync(host);
        }

        public static async Task RunAsync(IHost host) {

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var _cmds = serviceProvider.GetRequiredService<InteractionService>();
            var _settings = serviceProvider.GetRequiredService<SettingsHandler>();
            var _lavaNode = serviceProvider.GetRequiredService<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitAsync();
            serviceProvider.GetRequiredService<Core.Handlers.EventHandler>();

            await _client.LoginAsync(TokenType.Bot, _settings.Discord.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private static async void RunLavalink() {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var lavaFolder = Path.Combine(basePath, "Resources", "Victoria");
            var lavaJar = Path.Combine(lavaFolder, "Lavalink.jar");

            if (!File.Exists(lavaJar)) {
                Console.WriteLine($"Lavalink.jar not found at {lavaJar}");
                return;
            }

            var process = new Process();
            process.EnableRaisingEvents = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = lavaFolder;  // ✅ use the folder, not the jar
            process.StartInfo.FileName = "javaw";
            process.StartInfo.Arguments = $"-jar \"{lavaJar}\"";  // ✅ pass the jar as an argument
            process.Start();
            await Task.Delay(4000);
        }

    }
}
