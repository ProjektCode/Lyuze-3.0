using Discord;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;
using Lyuze.Core.Infrastructure.Database;
using Lyuze.Core.Models;
using MongoDB.Driver;

namespace Lyuze.Core.Features.Roles;

public sealed class ReactionRolesService {
    private readonly DiscordSocketClient _client;
    private readonly ILoggingService _logger;
    private readonly SettingsConfig _settings;
    private readonly DatabaseContext _db;
    private readonly ISettingsService _settingsService;

    // Cache: Key = MessageId + Emoji, Value = RoleId
    private readonly Dictionary<(ulong MessageId, string Emoji), ulong> _emojiToRoleId = new();

    private readonly ulong _roleIdToColorCycle = 1343897392293875753;

    public ReactionRolesService(
        DiscordSocketClient client,
        ILoggingService logger,
        SettingsConfig settingsConfig,
        DatabaseContext db,
        ISettingsService settingsService) {
        
        _client = client;
        _logger = logger;
        _settings = settingsConfig;
        _db = db;
        _settingsService = settingsService;

        _client.ReactionAdded += OnReactionAddedAsync;
        _client.ReactionRemoved += OnReactionRemovedAsync;
    }

    public async Task InitializeAsync(CancellationToken ct = default) {
        // Load from Database
        var roles = await _db.ReactionRoles.Find(_ => true).ToListAsync(ct);
        
        _emojiToRoleId.Clear();
        foreach (var rr in roles) {
            _emojiToRoleId[(rr.MessageId, rr.Emoji)] = rr.RoleId;
        }

        await _logger.LogInformationAsync("roles", $"Loaded {_emojiToRoleId.Count} reaction roles from database.");

        _ = RunRoleColorCycleAsync(ct);
    }

    public async Task AddReactionRoleAsync(string emoji, ulong roleId, ulong messageId) {
        if (string.IsNullOrWhiteSpace(emoji) || roleId == 0 || messageId == 0) return;

        // DB Insert
        var model = new ReactionRoleModel {
            Emoji = emoji,
            RoleId = roleId,
            MessageId = messageId
        };
        await _db.ReactionRoles.InsertOneAsync(model);

        // Update Cache
        _emojiToRoleId[(messageId, emoji)] = roleId;
        
        await _logger.LogInformationAsync("roles", $"Added reaction role: {emoji} -> {roleId}");
    }

    private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
        try {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) return;

            var emojiKey = GetEmojiKey(reaction.Emote);
            
            // Look up by (MessageId, Emoji)
            if (!_emojiToRoleId.TryGetValue((reaction.MessageId, emojiKey), out var roleId))
                return;

            var guildUser = ResolveGuildUser(channel, reaction.UserId, reaction.User.Value);
            if (guildUser == null) return;

            var role = guildUser.Guild.GetRole(roleId);
            if (role != null) {
                await guildUser.AddRoleAsync(role);
            }
        } catch (Exception ex) {
            await _logger.LogErrorAsync("roles", "Error handling ReactionAdded.", ex);
        }
    }

    private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
        try {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) return;

            var emojiKey = GetEmojiKey(reaction.Emote);

            if (!_emojiToRoleId.TryGetValue((reaction.MessageId, emojiKey), out var roleId))
                return;

            var guildUser = ResolveGuildUser(channel, reaction.UserId, reaction.User.Value);
            if (guildUser == null) return;

            var role = guildUser.Guild.GetRole(roleId);
            if (role != null) {
                await guildUser.RemoveRoleAsync(role);
            }
        } catch (Exception ex) {
            await _logger.LogErrorAsync("roles", "Error handling ReactionRemoved.", ex);
        }
    }

    private static SocketGuildUser? ResolveGuildUser(Cacheable<IMessageChannel, ulong> channel, ulong userId, IUser reactionUser) {
        if (reactionUser is SocketGuildUser sgu) return sgu;
        if (channel.Value is SocketGuildChannel guildChannel) return guildChannel.Guild.GetUser(userId);
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
                await role.ModifyAsync(r => r.Color = new Color((uint)Random.Shared.Next(0x1000000)));
            }
        } catch (OperationCanceledException) { } 
        catch (Exception ex) { await _logger.LogErrorAsync("roles", "Role color cycle failed.", ex); }
    }

    private static string GetEmojiKey(IEmote emote) =>
        emote switch {
            Emote custom => $"<:{custom.Name}:{custom.Id}>",
            Emoji emoji => emoji.Name,
            _ => emote.ToString()!
        };
}
