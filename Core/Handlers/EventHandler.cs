using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Database.Model;
using Lyuze.Core.Database.Services;
using Lyuze.Core.Services;
using Lyuze.Core.Services.Images;
using Lyuze.Core.Utilities;

namespace Lyuze.Core.Handlers {
    public class EventHandler {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _cmds;
        private readonly LevelingService _lvlService;
        private readonly ILoggingService _logger;

        public EventHandler(DiscordSocketClient client, InteractionService cmds, LevelingService levelingService, ILoggingService logger) {
            _client = client;
            _cmds = cmds;
            _lvlService = levelingService;
            _logger = logger;

            //Register Events
            _client.UserJoined += OnUserJoinedAsync;
            _client.Log += OnLogAsync;
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceivedAsync;
            _cmds.SlashCommandExecuted += OnSlashCommandAsync;
            _cmds.Log += OnCommandLogAsync;
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user) {
            ulong? roleID = SettingsHandler.Instance.IDs?.JoinRoleId;
            ulong? welcomeID = SettingsHandler.Instance.IDs?.WelcomeId;

            try {

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

                if (welcomeID.HasValue) {

                    var random = new Random();
                    var index = random.Next(SettingsHandler.Instance.WelcomeMessage.Count);
                    string path = await ImageGenerator.CreateBannerImageAsync(user, SettingsHandler.Instance.WelcomeMessage[index], "Welcome to the server.");

                    var channel = user.Guild.GetTextChannel(welcomeID.Value);
                    if (channel != null) await channel.SendFileAsync(path);
                    ImageUtils.DeleteImageFile(path);

                }

            } catch (Exception ex) {
                Console.WriteLine($"[EventHandler] Error has occured in OnUserJoined: {ex.Message}");
            }

        }

        private async Task<Task> OnLogAsync(LogMessage msg) {
            await _logger.LogAsync(msg.Source, msg.Severity, msg.Message);
            return Task.CompletedTask;
        }

        private Task OnCommandLogAsync(LogMessage msg) { 
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private async Task OnReadyAsync() {
            try {
                //var settings = SettingsHandler.LoadAsync();
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

        private async Task OnMessageReceivedAsync(SocketMessage arg) {
            try {
                if (arg is not SocketUserMessage message)
                    return;

                if (message.Author.IsBot || message.Channel is IDMChannel)
                    return;

                var ctx = new SocketCommandContext(_client, message);
                var user = ctx.User as SocketGuildUser;

                if (user == null)
                    return; // maybe DM or other channel

                if (!await Player.HasProfileAsync(user)) {
                    await Player.CreateProfileAsync(user);
                } else {
                    await _lvlService.MsgCoolDownAsync(message, ctx);
                }

                if (message.Content.Contains("https://discord.gg/", StringComparison.OrdinalIgnoreCase)
                    && !ctx.Guild.GetUser(user.Id).GuildPermissions.ManageMessages) {
                    await message.DeleteAsync();
                    await message.Channel.SendMessageAsync($"{user.Mention} cannot send discord invites.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"[OnMessageReceivedAsync] Error: {ex.Message}");
            }
        }



        private async Task OnSlashCommandAsync(SlashCommandInfo cmdInfo, IInteractionContext ctx, Discord.Interactions.IResult result) {
            if ((!result.IsSuccess)) {
                Console.WriteLine($"[Event Handler] Slash command {cmdInfo.Name} failed {result.ErrorReason}");
                return;
            }

            if (ctx.User is SocketGuildUser guildUser && !guildUser.IsBot) {
                //if (Player.CheckProfileAsync((SocketGuildUser)ctx.User)) {

                //}
                await _lvlService.GiveXP(guildUser, 10);
            }

        }

    }
}
