using Discord;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Infrastructure.DiscordNet.Handlers;
using Microsoft.Extensions.Hosting;

namespace Lyuze.Core.Infrastructure.DiscordNet;

/// <summary>
/// A hosted service that handles the Discord bot's startup and shutdown lifecycle.
/// This replaces the manual startup logic previously in Program.cs.
/// </summary>
public sealed class DiscordStartupService : BackgroundService {
    private readonly DiscordSocketClient _client;
    private readonly InteractionHandler _interactionHandler;
    private readonly Handlers.EventHandler _eventHandler;
    private readonly SettingsConfig _settings;
    private readonly ILoggingService _logger;

    public DiscordStartupService(
        DiscordSocketClient client,
        InteractionHandler interactionHandler,
        Handlers.EventHandler eventHandler,
        SettingsConfig settings,
        ILoggingService logger) {
        _client = client;
        _interactionHandler = interactionHandler;
        _eventHandler = eventHandler;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            await _logger.LogInformationAsync("startup", "Discord startup service is initializing...");

            // Initialize interaction handler (registers slash commands, hooks events)
            await _interactionHandler.InitAsync();

            // Login and start the client
            await _client.LoginAsync(TokenType.Bot, _settings.Discord.Token);
            await _client.StartAsync();

            await _logger.LogInformationAsync("startup", "Discord bot has started successfully.");

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        } catch (OperationCanceledException) {
            // Expected when the host is stopping
            await _logger.LogInformationAsync("startup", "Discord startup service is stopping...");
        } catch (Exception ex) {
            await _logger.LogCriticalAsync("startup", "Discord startup service encountered a fatal error.", ex);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        await _logger.LogInformationAsync("startup", "Discord bot is shutting down...");

        try {
            await _client.StopAsync();
            await _client.LogoutAsync();
        } catch (Exception ex) {
            await _logger.LogErrorAsync("startup", "Error during Discord client shutdown.", ex);
        }

        await base.StopAsync(cancellationToken);
    }
}
