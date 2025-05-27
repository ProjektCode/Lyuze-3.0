using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Services;
using Lyuze.Core.Utilities;

namespace Lyuze.Core.Handlers {
    public class EventHandler {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _cmds;

        public EventHandler(DiscordSocketClient client, InteractionService cmds) {
            _client = client;
            _cmds = cmds;

            //Register Events
            _client.UserJoined += OnUserJoinedAsync;
            _client.Log += OnLogAsync;
            _cmds.Log += OnCommandLogAsync;
            _client.Ready += OnReadyAsync;

        }

        private async Task OnUserJoinedAsync(SocketGuildUser user) {
            ulong? roleID = SettingsHandler.Instance.IDs?.JoinRoleId;

            if (roleID.HasValue && roleID.Value > 0) {

                try {
                    var role = user.Guild.GetRoleAsync(roleID.Value);
                    if (role != null) {
                        await user.AddRoleAsync(roleID.Value);
                        Console.WriteLine($"Assigned role 'Gamer' to {user.Username}");
                    }

                } catch (Exception ex) { 
                    Console.WriteLine(ex.ToString());
                }

            }

        }

        private Task OnLogAsync(LogMessage msg) {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private Task OnCommandLogAsync(LogMessage msg) { 
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private async Task OnReadyAsync() {
            try {
                await _cmds.RegisterCommandsToGuildAsync(SettingsHandler.Instance.Discord.GuildId);

                await _client.SetStatusAsync(UserStatus.Online);
                var i = MasterUtilities.Instance.RandomListIndex(SettingsHandler.Instance.Status ?? ["Online", "Idle", "Do Not Disturb"]);
                var t = new Timer(async __ => { 
                    await _client.SetGameAsync(MasterUtilities.Instance.sList.ElementAtOrDefault(i), type: ActivityType.Listening);
                    i = i + 1 == MasterUtilities.Instance.sList.Count ? 0 : i + 1;
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(120));

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
