using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Features.Profiles;
using Lyuze.Core.Features.Roles;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Shared.Images;
using Lyuze.Core.Shared.Images.Primitives;

namespace Lyuze.Core.Infrastructure.DiscordNet.Handlers {
    public class EventHandler {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly ILoggingService _logger;
        private readonly SettingsConfig _settings;
        private readonly LevelingService _levelingService;
        private readonly IPlayerService _playerService;
        private readonly IStatusProvider _statusProvider;
        private readonly ReactionRolesService _reactionRolesService;
        private readonly ImageFetcher _imageFetcher;
        private readonly ColorUtils _colorUtils;

        private CancellationTokenSource? _statusCts;
        private Task? _statusTask;

        public EventHandler(DiscordSocketClient client, InteractionService interactions, ILoggingService logger, SettingsConfig settings, LevelingService levelingService,
            IPlayerService playerService, IStatusProvider statusProvider, ReactionRolesService reactionRolesService, ImageFetcher imageFetcher, ColorUtils colorUtils) {

            _client = client;
            _interactions = interactions;
            _logger = logger;
            _settings = settings;
            _levelingService = levelingService;
            _playerService = playerService;
            _statusProvider = statusProvider;
            _reactionRolesService = reactionRolesService;
            _imageFetcher = imageFetcher;
            _colorUtils = colorUtils;

            // Register events
            _client.UserJoined += OnUserJoinedAsync;
            _client.Log += OnLogAsync;
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceivedAsync;

            _interactions.SlashCommandExecuted += OnSlashCommandAsync;
            _interactions.Log += OnCommandLogAsync;
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user) {
            ulong? roleId = _settings.IDs?.JoinRoleId;
            ulong? welcomeChannelId = _settings.IDs?.WelcomeId;

            if(!await _playerService.HasProfileAsync(user)) await _playerService.CreateProfileAsync(user);
            var player = await _playerService.GetUserAsync(user);

            try {
                // Assign join role
                if (roleId is > 0) {
                    try {
                        var role = user.Guild.GetRole(roleId.Value);
                        if (role != null) {
                            await user.AddRoleAsync(roleId.Value);
                            await _logger.LogInformationAsync("join",
                                $"Assigned join role '{role.Name}' to {user.Username} ({user.Id}).");
                        }
                    } catch (Exception ex) {
                        await _logger.LogCriticalAsync("join",
                            $"Failed to assign join role to {user.Username} ({user.Id}): {ex.Message}", ex);
                    }
                }

                // Send welcome banner
                if (welcomeChannelId is > 0 && _settings.WelcomeMessage.Count > 0) {
                    int index = Random.Shared.Next(_settings.WelcomeMessage.Count);
                    string message = _settings.WelcomeMessage[index];

                    var img = await ImageGenerator.CreateWelcomeBannerAsync(
                        user,
                        player.Background,
                        message,
                        "Welcome to the server.",
                        _imageFetcher,
                        _colorUtils);

                    var channel = user.Guild.GetTextChannel(welcomeChannelId.Value);
                    if (channel != null) {
                        if (img is null) {
                            await channel.SendMessageAsync($"Welcome {user.Mention}!");
                            await _logger.LogWarningAsync("join", $"Failed to generate welcome banner for {user.Username} ({user.Id}) because img was null. sent text-only welcome.");
                            return;
                        }
                        await using var ms = new MemoryStream(img);
                        await channel.SendFileAsync(ms, $"welcome-{user.Username}.png");
                    }

                }
            } catch (Exception ex) {
                await _logger.LogCriticalAsync("join",
                    $"Unhandled exception in OnUserJoinedAsync for {user.Username} ({user.Id}): {ex.Message}", ex);
            }
        }

        private Task OnLogAsync(LogMessage msg) {
            // Pipe Discord.Net logs into your logging service
            return _logger.LogAsync(
                msg.Source,
                msg.Severity,
                msg.Message ?? string.Empty,
                msg.Exception);
        }

        private Task OnCommandLogAsync(LogMessage msg) {
            // You can route this through your logger too if you want
            return _logger.LogAsync(
                msg.Source,
                msg.Severity,
                msg.Message ?? string.Empty,
                msg.Exception);
        }

        private async Task OnReadyAsync() {
            try {
                await RegisterCommandsAsync();
                await SetOnlineAsync();

                StartStatusRotation(); // fire-and-forget background loop (tracked)
                await _reactionRolesService.InitializeAsync();

                await _logger.LogInformationAsync("discord", "Bot is ready and commands are registered.");
            } catch (Exception ex) {
                await _logger.LogCriticalAsync("discord", $"OnReady: {ex.Message}", ex);
            }
        }


        private async Task OnMessageReceivedAsync(SocketMessage rawMessage) {
            try {
                if (rawMessage is not SocketUserMessage message)
                    return;

                if (message.Author.IsBot || message.Channel is IDMChannel)
                    return;

                var ctx = new SocketCommandContext(_client, message);

                if (ctx.User is not SocketGuildUser user)
                    return;

                // Ensure profile or apply leveling
                if (!await _playerService.HasProfileAsync(user)) {
                    await _playerService.CreateProfileAsync(user);
                } else {
                    await _levelingService.MsgCoolDownAsync(message, ctx);
                }

                // Basic anti-invite check
                if (message.Content.Contains("https://discord.gg/", StringComparison.OrdinalIgnoreCase)
                    && !user.GuildPermissions.ManageMessages) {

                    await message.DeleteAsync();
                    await message.Channel.SendMessageAsync($"{user.Mention} cannot send discord invites.");
                    await _logger.LogInformationAsync("discord",
                        $"Deleted invite link from {user.Username} ({user.Id}) in #{message.Channel.Name}.");
                }
            } catch (Exception ex) {
                await _logger.LogCriticalAsync("discord",
                    $"OnMessageReceived: {ex.Message}", ex);
            }
        }

        private async Task OnSlashCommandAsync(
            SlashCommandInfo cmdInfo,
            IInteractionContext ctx,
            Discord.Interactions.IResult result) {

            if (!result.IsSuccess) {
                // Use Warning instead of Critical for typical command errors
                await _logger.LogAsync(
                    "discord",
                    result.Error == InteractionCommandError.Exception ? LogSeverity.Error : LogSeverity.Warning,
                    $"Slash command `{cmdInfo.Name}` failed: {result.Error} - {result.ErrorReason}");
                return;
            }

            if (ctx.User is SocketGuildUser guildUser && !guildUser.IsBot) {
                await _levelingService.GiveXPAsync(guildUser, 10);
            }
        }

        private async Task RegisterCommandsAsync() {
            await _interactions.RegisterCommandsToGuildAsync(_settings.Discord.GuildId);
        }

        private async Task SetOnlineAsync() {
            await _client.SetStatusAsync(UserStatus.Online);
        }

        private void StartStatusRotation() {
            // Stop an existing loop if Ready fires again for any reason
            StopStatusRotation();

            _statusCts = new CancellationTokenSource();
            _statusTask = RunStatusRotationAsync(_statusCts.Token);
        }

        private void StopStatusRotation() {
            if (_statusCts is null) return;

            try { _statusCts.Cancel(); } catch { /* ignore */ } finally {
                _statusCts.Dispose();
                _statusCts = null;
            }
        }

        private async Task RunStatusRotationAsync(CancellationToken token) {
            var statuses = _settings.Status;
            if (statuses == null || statuses.Count == 0) {
                await _logger.LogWarningAsync("discord", "Status list is empty; skipping status rotation.");
                return;
            }

            int i = _statusProvider.GetRandomStatusIndex(statuses);

            // Set an initial activity immediately
            await SetActivitySafeAsync(statuses[i]);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(120));

            while (!token.IsCancellationRequested) {
                try {
                    // Wait for the next tick
                    if (!await timer.WaitForNextTickAsync(token))
                        break;

                    i = (i + 1) % statuses.Count;
                    await SetActivitySafeAsync(statuses[i]);
                } catch (OperationCanceledException) {
                    break;
                } catch (Exception ex) {
                    await _logger.LogCriticalAsync("discord", $"Exception in status rotation: {ex.Message}", ex);
                }
            }
        }

        private async Task SetActivitySafeAsync(string? statusText) {
            if (string.IsNullOrWhiteSpace(statusText))
                return;

            // choose whatever activity type you want
            await _client.SetGameAsync(statusText, type: ActivityType.Listening);
        }


    }
}
