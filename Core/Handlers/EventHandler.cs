using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Configuration;
using Lyuze.Core.Services.Database;
using Lyuze.Core.Services.Images;
using Lyuze.Core.Services.Interfaces;
using Lyuze.Core.Utilities;

namespace Lyuze.Core.Handlers {
    public class EventHandler {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly ILoggingService _logger;
        private readonly SettingsConfig _settings;
        private readonly LevelingService _levelingService;
        private readonly MasterUtilities _utils;
        private readonly IPlayerService _playerService;

        private Timer? _statusTimer;

        public EventHandler(DiscordSocketClient client, InteractionService interactions, ILoggingService logger, SettingsConfig settings, LevelingService levelingService, MasterUtilities utils, IPlayerService playerService) {

            _client = client;
            _interactions = interactions;
            _logger = logger;
            _settings = settings;
            _levelingService = levelingService;
            _utils = utils;

            // Register events
            _client.UserJoined += OnUserJoinedAsync;
            _client.Log += OnLogAsync;
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceivedAsync;

            _interactions.SlashCommandExecuted += OnSlashCommandAsync;
            _interactions.Log += OnCommandLogAsync;
            _playerService = playerService;
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user) {
            ulong? roleId = _settings.IDs?.JoinRoleId;
            ulong? welcomeChannelId = _settings.IDs?.WelcomeId;

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

                    string path = await ImageGenerator.CreateBannerImageAsync(
                        user,
                        message,
                        "Welcome to the server.");

                    var channel = user.Guild.GetTextChannel(welcomeChannelId.Value);
                    if (channel != null) {
                        await channel.SendFileAsync(path);
                    }

                    ImageUtils.DeleteImageFile(path);
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
                await _interactions.RegisterCommandsToGuildAsync(_settings.Discord.GuildId);

                await _client.SetStatusAsync(UserStatus.Online);

                // pick a starting index safely
                var statuses = _settings.Status ?? ["Online", "Idle", "Do Not Disturb"];
                int i = _utils.RandomListIndex(statuses);

                // Keep a reference so the timer isn't GC'd
                _statusTimer = new Timer(async _ => {
                    try {
                        var text = _utils.StatusList[i];
                        if (!string.IsNullOrWhiteSpace(text)) {
                            await _client.SetGameAsync(text, type: ActivityType.Listening);
                        }

                        i = (i + 1) % _utils.StatusList.Count;
                    } catch (Exception ex) {
                        await _logger.LogCriticalAsync("discord",
                            $"Exception in status rotation timer: {ex.Message}", ex);
                    }
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(120));

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
    }
}
