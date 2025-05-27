using Discord;
using Discord.WebSocket;
using Lyuze.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lyuze.Core.Services {
    public class ReactionRolesService {
        private readonly DiscordSocketClient _client;
        private readonly Dictionary<ulong, Dictionary<string, ulong>> _reactionRoles = [];
        private readonly ulong _roleId = 1343897392293875753;
        private readonly Timer _timer;
        private readonly Random _random = new();

        public ReactionRolesService(DiscordSocketClient client) {
            _client = client;
            _client.ReactionAdded += OnReactionAddedAsync;
            _client.ReactionRemoved += OnReactionRemovedAsync;

            InitializeReactionRoles();

            _timer = new Timer(ChangeRoleColor, null, TimeSpan.Zero, TimeSpan.FromMinutes(45));
        }

        // Add a reaction-role mapping
        public void AddReactionRole(string emoji, ulong roleId) {
            try {

                if (SettingsHandler.Instance.IDs?.ReactionRoleMessageId == null) {
                    Console.WriteLine("[ReactionRoleHandler] Reaction roles message not found.");
                    return;
                }

                var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;

                if (!_reactionRoles.TryGetValue(messageId, out var value)) {
                    value = [];
                    _reactionRoles[messageId] = value;
                }

                value[emoji] = roleId;
            } catch (Exception ex) {
                Console.WriteLine($"[ReactionRoleHandler] AddReactionRole Exception: {ex}");
            }
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            try {
                if (reaction.User.IsSpecified && !reaction.User.Value.IsBot) {
                    if (SettingsHandler.Instance.IDs?.ReactionRoleMessageId == null) {
                        Console.WriteLine("[ReactionRoleHandler] Reaction roles message not found.");
                        return;
                    }

                    var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                    var emojiKey = GetEmojiKey(reaction.Emote);

                    if (_reactionRoles.TryGetValue(messageId, out var emojiMap) && emojiMap.TryGetValue(emojiKey, out var roleId)) {
                        SocketGuildUser? guildUser = reaction.User.Value as SocketGuildUser;

                        if (guildUser == null && channel.Value is SocketGuildChannel guildChannel) {
                            guildUser = guildChannel.Guild.GetUser(reaction.UserId);
                        }

                        var role = (channel.Value as SocketGuildChannel)?.Guild.GetRole(roleId);
                        if (guildUser != null && role != null) {
                            await guildUser.AddRoleAsync(role);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"[ReactionRoleHandler] OnReactionAddedAsync Exception: {ex}");
            }
        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            try {
                if (reaction.User.IsSpecified && !reaction.User.Value.IsBot) {
                    if (SettingsHandler.Instance.IDs?.ReactionRoleMessageId == null) {
                        Console.WriteLine("[ReactionRoleHandler] Reaction roles message not found.");
                        return;
                    }

                    var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                    var emojiKey = GetEmojiKey(reaction.Emote);

                    if (_reactionRoles.TryGetValue(messageId, out var emojiMap) && emojiMap.TryGetValue(emojiKey, out var roleId)) {
                        SocketGuildUser? guildUser = reaction.User.Value as SocketGuildUser;

                        if (guildUser == null && channel.Value is SocketGuildChannel guildChannel) {
                            guildUser = guildChannel.Guild.GetUser(reaction.UserId);
                        }

                        var role = (channel.Value as SocketGuildChannel)?.Guild.GetRole(roleId);
                        if (guildUser != null && role != null) {
                            await guildUser.RemoveRoleAsync(role);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"[ReactionRoleHandler] OnReactionRemovedAsync Exception: {ex}");
            }
        }

        private void InitializeReactionRoles() {
            try {
                if (SettingsHandler.Instance.IDs?.ReactionRoleMessageId == null) {
                    Console.WriteLine("[ReactionRoleHandler] Reaction roles message not found.");
                    return;
                }

                var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;

                if (!_reactionRoles.ContainsKey(messageId)) {
                    _reactionRoles[messageId] = [];
                    Console.WriteLine("[ReactionRoleHandler] Initializing Reaction Roles");
                }

                // Load roles from settings (must be List<ReactionRole>)
                foreach (var rr in SettingsHandler.Instance.ReactionRoles) {
                    var emojiKey = rr.Emoji;
                    AddReactionRole(emojiKey, rr.RoleId);
                }

            } catch (Exception ex) {
                Console.WriteLine($"[ReactionRoleHandler] InitializeReactionRoles Exception: {ex}");
            }
        }

        private async void ChangeRoleColor(object? state) {
            var guild = _client.Guilds.FirstOrDefault();
            if (guild == null) return;

            var role = guild.GetRole(_roleId);
            if (role == null) return;

            var color = new Color((uint)_random.Next(0x1000000));
            await role.ModifyAsync(r => r.Color = color);
        }

        private static string GetEmojiKey(IEmote emote) {
            return emote switch {
                Emote custom => $"<:{custom.Name}:{custom.Id}>",
                Emoji emoji => emoji.Name,
                _ => emote.ToString()!
            };
        }
    }

    // Expected format in SettingsHandler.Instance.ReactionRoles
    public class ReactionRole {
        public string Emoji { get; set; } = string.Empty;
        public ulong RoleId { get; set; }
    }
}
