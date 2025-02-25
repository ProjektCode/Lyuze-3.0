using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lyuze.Core.Handlers {
    public class ReactionRoleHandler {
        private readonly DiscordSocketClient _client;
        private readonly Dictionary<ulong, Dictionary<string, ulong>> _reactionRoles = [];

        private readonly ulong _roleId = 1343897392293875753;
        private readonly Timer _timer;
        private readonly Random _random = new();

        public ReactionRoleHandler(DiscordSocketClient client) {
            _client = client;
            _client.ReactionAdded += OnReactionAddedAsync;
            _client.ReactionRemoved += OnReactionRemovedAsync;

            // Initialize reaction roles from settings
            InitializeReactionRoles();

            _timer = new Timer(ChangeRoleColor, null, TimeSpan.Zero, TimeSpan.FromMinutes(45));
        }

        // Add reaction role mapping
        public void AddReactionRole(string emoji, ulong roleId) {

            try {
                var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                if (!_reactionRoles.TryGetValue(messageId, out Dictionary<string, ulong>? value)) {
                    value = [];
                    _reactionRoles[messageId] = value;
                }

                value[emoji] = roleId;
                _reactionRoles[messageId] = value;
            } catch (Exception ex) { Console.WriteLine(ex.ToString()); };

        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {

            try {

                if (reaction.User.IsSpecified && !reaction.User.Value.IsBot) {
                    var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                    var emoji = reaction.Emote.ToString();

                    if (_reactionRoles.TryGetValue(messageId, out var value) && value.TryGetValue(emoji, out var roleId)) {
                        var guildUser = reaction.User.Value as SocketGuildUser;
                        var role = (reaction.Channel as SocketGuildChannel)?.Guild.GetRole(roleId);

                        if (role != null) {
                            await guildUser.AddRoleAsync(role);
                        }
                    } else { Console.WriteLine("reaction add role event false"); }
                }

            } catch (Exception ex) { Console.WriteLine(ex.ToString()); };

        }

        private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            
            try {

                if (reaction.User.IsSpecified && !reaction.User.Value.IsBot) {
                    var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                    var emoji = reaction.Emote.ToString();

                    if (_reactionRoles.TryGetValue(messageId, out var value) && value.TryGetValue(emoji, out var roleId)) {
                        var guildUser = reaction.User.Value as SocketGuildUser;
                        var role = (reaction.Channel as SocketGuildChannel)?.Guild.GetRole(roleId);

                        if (role != null) {
                            await guildUser.RemoveRoleAsync(role);
                        }
                    } else { Console.WriteLine("reaction remove role false"); }
                }

            } catch (Exception ex) { Console.WriteLine(ex.ToString()); };

        }

        // Initialize reaction roles from settings
        private void InitializeReactionRoles() {

            try {

                var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;

                if (!_reactionRoles.ContainsKey(messageId)) {
                    _reactionRoles[messageId] = [];
                    Console.WriteLine("Initializing Reaction Roles");
                } else {
                    Console.WriteLine("Else role initialization");
                }

            } catch (Exception ex) { Console.WriteLine(ex.ToString()); };

        }

        private async void ChangeRoleColor(object state) {
            var guild = _client.Guilds.FirstOrDefault();
            if (guild == null) return;

            var role = guild.GetRole(_roleId);
            if (role == null) return;

            var color = new Color((uint)_random.Next(0x1000000));
            await role.ModifyAsync(r => r.Color = color);
        }

    }
}
