using Discord;
using Discord.WebSocket;
using Lyuze.Core.Configuration;
using Lyuze.Core.Services.Interfaces;

namespace Lyuze.Core.Services;

public sealed class ReactionRolesService {
    private readonly DiscordSocketClient _client;
    private readonly ILoggingService _logger;
    private readonly SettingsConfig _settings;

    // Since you only support ONE reaction role message id in settings, keep this simple:
    private readonly Dictionary<string, ulong> _emojiToRoleId = new(StringComparer.Ordinal);

    // Optional: move these into settings later
    private readonly ulong _roleIdToColorCycle = 1343897392293875753;

    public ReactionRolesService(
        DiscordSocketClient client,
        ILoggingService logger,
        SettingsConfig settingsConfig
    ) {
        _client = client;
        _logger = logger;
        _settings = settingsConfig;

        // Hook events (safe in constructor)
        _client.ReactionAdded += OnReactionAddedAsync;
        _client.ReactionRemoved += OnReactionRemovedAsync;
    }

    /// <summary>
    /// Call once when the bot is ready (after settings are loaded).
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default) {
        if (_settings.IDs?.ReactionRoleMessageId is null or 0) {
            await _logger.LogWarningAsync("roles", "Reaction role message id not configured.");
            return;
        }

        _emojiToRoleId.Clear();

        if (_settings.ReactionRoles is null || _settings.ReactionRoles.Count == 0) {
            await _logger.LogInformationAsync("roles", "No reaction roles found in settings.");
            return;
        }

        foreach (var rr in _settings.ReactionRoles) {
            if (string.IsNullOrWhiteSpace(rr.Emoji) || rr.RoleId == 0)
                continue;

            _emojiToRoleId[rr.Emoji] = rr.RoleId;
        }

        await _logger.LogInformationAsync("roles", $"Loaded {_emojiToRoleId.Count} reaction roles from settings.");

        // If you want the role color cycling, start it here (optional).
        _ = RunRoleColorCycleAsync(ct);
    }

    public Task AddReactionRoleAsync(string emoji, ulong roleId) {
        if (string.IsNullOrWhiteSpace(emoji) || roleId == 0)
            return Task.CompletedTask;

        _emojiToRoleId[emoji] = roleId;
        return Task.CompletedTask;
    }

    private async Task OnReactionAddedAsync(
        Cacheable<IUserMessage, ulong> cache,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    ) {
        try {
            if (!reaction.User.IsSpecified) return;
            if (reaction.User.Value.IsBot) return;

            if (_settings.IDs?.ReactionRoleMessageId is null or 0) return;

            // Only react to the configured message
            var configuredMessageId = _settings.IDs.ReactionRoleMessageId;
            if (reaction.MessageId != configuredMessageId) return;

            var emojiKey = GetEmojiKey(reaction.Emote);

            if (!_emojiToRoleId.TryGetValue(emojiKey, out var roleId))
                return;

            var guildUser = await ResolveGuildUserAsync(channel, reaction.UserId, reaction.User.Value);
            if (guildUser == null) return;

            var guild = (channel.Value as SocketGuildChannel)?.Guild ?? guildUser.Guild;
            var role = guild.GetRole(roleId);
            if (role == null) return;

            await guildUser.AddRoleAsync(role);
        } catch (Exception ex) {
            await _logger.LogErrorAsync("roles", "Error handling ReactionAdded.", ex);
        }
    }

    private async Task OnReactionRemovedAsync(
        Cacheable<IUserMessage, ulong> cache,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    ) {
        try {
            if (!reaction.User.IsSpecified) return;
            if (reaction.User.Value.IsBot) return;

            if (_settings.IDs?.ReactionRoleMessageId is null or 0) return;

            var configuredMessageId = _settings.IDs.ReactionRoleMessageId;
            if (reaction.MessageId != configuredMessageId) return;

            var emojiKey = GetEmojiKey(reaction.Emote);

            if (!_emojiToRoleId.TryGetValue(emojiKey, out var roleId))
                return;

            var guildUser = await ResolveGuildUserAsync(channel, reaction.UserId, reaction.User.Value);
            if (guildUser == null) return;

            var guild = (channel.Value as SocketGuildChannel)?.Guild ?? guildUser.Guild;
            var role = guild.GetRole(roleId);
            if (role == null) return;

            await guildUser.RemoveRoleAsync(role);
        } catch (Exception ex) {
            await _logger.LogErrorAsync("roles", "Error handling ReactionRemoved.", ex);
        }
    }

    private static async Task<SocketGuildUser?> ResolveGuildUserAsync(
        Cacheable<IMessageChannel, ulong> channel,
        ulong userId,
        IUser reactionUser
    ) {
        if (reactionUser is SocketGuildUser sgu)
            return sgu;

        if (channel.Value is SocketGuildChannel guildChannel)
            return guildChannel.Guild.GetUser(userId);

        return null;
    }

    private async Task RunRoleColorCycleAsync(CancellationToken ct) {
        try {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(45));

            while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct)) {
                var guild = _client.Guilds.FirstOrDefault();
                if (guild == null) continue;

                var role = guild.GetRole(_roleIdToColorCycle);
                if (role == null) continue;

                var color = new Color((uint)Random.Shared.Next(0x1000000));
                await role.ModifyAsync(r => r.Color = color);
            }
        } catch (OperationCanceledException) {
            // ok
        } catch (Exception ex) {
            await _logger.LogErrorAsync("roles", "Role color cycle failed.", ex);
        }
    }

    private static string GetEmojiKey(IEmote emote) =>
        emote switch {
            Emote custom => $"<:{custom.Name}:{custom.Id}>",
            Emoji emoji => emoji.Name,
            _ => emote.ToString()!
        };
}
