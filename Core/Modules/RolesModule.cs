using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Handlers;
using Lyuze.Core.Utilities;
using System.Data;

namespace Lyuze.Core.Modules {
    public class RolesModule(ReactionRoleHandler reactionRoleHandler) : InteractionModuleBase<SocketInteractionContext> {

        private readonly ReactionRoleHandler _reactionRoleHandler = reactionRoleHandler;

        [SlashCommand("remove_role", "Remove a specified role from a user")]
        public async Task RemoveRole(IUser user) {
            var guildUser = (SocketGuildUser)user;
            var userRoles = guildUser.Roles.Where(role => !role.IsEveryone).ToList(); // Exclude @everyone role

            if (userRoles.Count == 0) {
                await RespondAsync($"{user.Username} does not have any removable roles.");
                return;
            }

            var selectMenuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Choose a role to remove")
                .WithCustomId("select_role_to_remove")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var role in userRoles) {
                selectMenuBuilder.AddOption(role.Name, role.Id.ToString(), $"Remove {role.Name} from {user.Username}");
            }

            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(selectMenuBuilder);

            await RespondAsync($"Select a role to remove from {user.Username}:", components: componentBuilder.Build(), ephemeral: true);
            await MasterUtilities.DelayAndDeleteResponseAsync(Context);
        }

        [ComponentInteraction("select_role_to_remove")]
        public async Task HandleRoleSelection(string[] selectedValues) {
            if (selectedValues.Length == 0) {
                await RespondAsync("No role selected.", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                return;
            }

            if (!ulong.TryParse(selectedValues[0], out ulong roleId)) {
                await RespondAsync("Invalid role selected.", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                return;
            }

            var guildUser = (SocketGuildUser)Context.User;
            var roleToRemove = Context.Guild.GetRole(roleId);

            if (roleToRemove == null) {
                await RespondAsync("Role not found.", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
                return;
            }

            await guildUser.RemoveRoleAsync(roleToRemove);
            await RespondAsync($"Role '{roleToRemove.Name}' has been removed from {guildUser.Username}.", ephemeral: true);
            await MasterUtilities.DelayAndDeleteResponseAsync(Context);
        }

        [SlashCommand("setup_reaction_roles", "Set up the reaction roles embed message.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupReactionRoles() {
            try {

                await DeferAsync();

                var embed = new EmbedBuilder()
                    .WithTitle("ROLES")
                    .WithDescription("React with the following emojis to get the corresponding roles:\n" +
                                     "<:GachaUpdates:1343843757623349369> - Gacha Updates\n" +
                                     "<:GameUpdates:1343843759540015104> - Game Updates\n" +
                                     "<:Weeb:1343843763105038457> - Weeb\n" +
                                     "---------------------------------------------\n" +
                                     "React to the following emojis to get a colored name:\n" +
                                     "❤️ - Red Name\n" +
                                     "💙 - Blue Name\n" +
                                     "💜 - Purple Name\n" +
                                     "\U0001f5a4 - Black Name\n" +
                                     "💛 - Yellow Name\n" +
                                     "💚 - Green Name\n" +
                                     "\U0001f9e1 - Orange Name\n" +
                                     "\U0001f90e - Brown Name\n" +
                                     "\U0001f90d - White Name\n" +
                                     "\U0001fa77 - Pink Name\n" +
                                     "\U0001fa76 - Grey Name\n" +
                                     "\U0001fa75 - Light Blue Name\n" +
                                     "🌈 - Rainbow Name\n")
                    .WithColor(Color.DarkRed)
                    .WithImageUrl("https://i.imgur.com/w364R6i.png")
                    .Build();

                var message = await Context.Channel.SendMessageAsync(embed: embed);

                //Saves the message id for settings
                SettingsHandler.Instance.IDs.ReactionRoleMessageId = message.Id;
                SettingsHandler.Instance.SaveSettings();

                // Create an array of emojis
                var emojis = new IEmote[] {
                Emote.Parse("<:GachaUpdates:1343843757623349369>"), //Gacha Updates
                Emote.Parse("<:GameUpdates:1343843759540015104>"), //Game Updates
                Emote.Parse("<:Weeb:1343843763105038457>"), //WeebII
                new Emoji("❤️"), // Red Heart
                new Emoji("💙"), // Blue Heart
                new Emoji("💜"), // Purple Heart
                new Emoji("\U0001f5a4"), // Black Heart
                new Emoji("💛"), // Yellow Heart
                new Emoji("💚"), // Green Heart
                new Emoji("\U0001f9e1"), // Orange Heart
                new Emoji("\U0001f90e"), // Brown Heart
                new Emoji("\U0001f90d"), // White Heart
                new Emoji("\U0001fa77"), // Pink Heart
                new Emoji("\U0001fa76"), // Grey Heart
                new Emoji("\U0001fa75") // Light Blue Heart
            };

                // Add all reactions at once
                await message.AddReactionsAsync(emojis);
                await FollowupAsync("Reaction Roles have been made.", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);

            }catch(Exception ex) {
                Console.WriteLine(ex.ToString());
                await FollowupAsync("An error occured. Command failed.");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }

        }

        [SlashCommand("add_reaction_roles", "Edit the reaction roles embed message to add a new role.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EditReactionRoles([Summary(description: "Select an emoji for the role")] string selectedEmoji, [Summary(description: "Select a role")] SocketRole role) {
            await DeferAsync();

            var channel = Context.Channel;
            var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
            var message = await channel.GetMessageAsync(messageId) as IUserMessage;

            if (message == null) {
                await FollowupAsync("Reaction roles message not found.", ephemeral: true);
                return;
            }

            var embed = message.Embeds.FirstOrDefault();
            if (embed == null) {
                await FollowupAsync("No embed found in the reaction roles message.", ephemeral: true);
                return;
            }

            var newDescription = embed.Description + $"\n{selectedEmoji} - {role.Name}";

            var newEmbed = new EmbedBuilder()
                .WithTitle(embed.Title)
                .WithDescription(newDescription)
                .WithColor(embed.Color.Value)
                .WithImageUrl(embed.Image?.Url)
                .Build();

            await message.ModifyAsync(msg => msg.Embed = newEmbed);

            // Add the new reaction
            var emote = new Emoji(selectedEmoji);
            await message.AddReactionAsync(emote);

            // Register the new reaction role
            _reactionRoleHandler.AddReactionRole(selectedEmoji, role.Id);

            await FollowupAsync("Reaction role has been added and embed updated.", ephemeral: true);
        }

        [SlashCommand("add_bot_reaction", "Use this command to ONLY add a new reaction, to setup a reaction role use add_reaction_roles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddReactionRole([Summary(description: "Select an emoji for the role")] string selectedEmoji, [Summary(description: "Select a role")] SocketRole role) {
            await DeferAsync();

            try {

                var channel = Context.Channel;
                var messageId = SettingsHandler.Instance.IDs.ReactionRoleMessageId;
                var message = await channel.GetMessageAsync(messageId) as IUserMessage;

                var emote = new Emoji(selectedEmoji);
                await message.AddReactionAsync(emote);
                // Register the new reaction role
                _reactionRoleHandler.AddReactionRole(selectedEmoji, role.Id);

                await FollowupAsync("Reaction has been added.", ephemeral: true);
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);

            } catch(Exception ex) {
                Console.WriteLine(ex.Message.ToString());
                await FollowupAsync($"An error has occured: {ex.Message.ToString()}");
                await MasterUtilities.DelayAndDeleteResponseAsync(Context);
            }
        }

    }
}
