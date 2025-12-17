using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Configuration;
using Lyuze.Core.Services;
using Lyuze.Core.Services.Extensions;
using Lyuze.Core.Services.Interfaces;
using Lyuze.Core.Utilities;
using System.Data;

namespace Lyuze.Core.Modules {
    public class RolesModule(ReactionRolesService reactionRoleHandler, SettingsConfig settingsConfig, ISettingsService settingsService) : InteractionModuleBase<SocketInteractionContext> {

        private readonly ReactionRolesService _reactionRoleHandler = reactionRoleHandler;
        private readonly SettingsConfig _settings = settingsConfig;
        private readonly ISettingsService _settingsService = settingsService;

        [SlashCommand("remove_role", "Remove a specified role from a user")]
        public async Task RemoveRole(IUser user) {
            var guildUser = (SocketGuildUser)user;
            var author = (SocketGuildUser)Context.User;

            if(author.Id != guildUser.Id) {
                if(!author.GuildPermissions.ManageRoles) {
                    await RespondAsync("You do not have the required permissions to remove a role from another other.");
                    return;
                }
            }

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
            await Context.DelayDeleteOriginalAsync();
        }

        [ComponentInteraction("select_role_to_remove")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SelectRoleRemoval(string[] selectedValues) {
            if (selectedValues.Length == 0) {
                await RespondAsync("No role selected.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            if (!ulong.TryParse(selectedValues[0], out ulong roleId)) {
                await RespondAsync("Invalid role selected.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            var guildUser = (SocketGuildUser)Context.User;
            var roleToRemove = Context.Guild.GetRole(roleId);

            if (roleToRemove == null) {
                await RespondAsync("Role not found.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
                return;
            }

            await guildUser.RemoveRoleAsync(roleToRemove);
            await RespondAsync($"Role '{roleToRemove.Name}' has been removed from {guildUser.Username}.", ephemeral: true);
            await Context.DelayDeleteOriginalAsync();
        }

        [SlashCommand("setup_reaction_roles", "Set up the reaction roles embed message.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupReactionRoles() {
            try {
                await DeferAsync();

                var guild = Context.Guild;

                var lines = new List<string>();
                var emotes = new List<IEmote>();

                foreach (var entry in _settings.ReactionRoles) {
                    string roleName = guild.GetRole(entry.RoleId)?.Name ?? "(unknown role)";
                    lines.Add($"{entry.Emoji} - {roleName}");

                    try {
                        if (entry.Emoji.StartsWith("<:")) {
                            emotes.Add(Emote.Parse(entry.Emoji));
                        } else {
                            emotes.Add(new Emoji(entry.Emoji));
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"Failed to parse emoji '{entry.Emoji}': {ex.Message}");
                    }
                }

                var embed = new EmbedBuilder()
                    .WithTitle("ROLES")
                    .WithDescription(string.Join("\n", lines))
                    .WithColor(Color.DarkRed)
                    .WithImageUrl("https://i.imgur.com/w364R6i.png")
                    .Build();

                var message = await Context.Channel.SendMessageAsync(embed: embed);

                if (_settings.IDs == null)
                    _settings.IDs = new IDs();

                _settings.IDs.ReactionRoleMessageId = message.Id;
                await _settingsService.SaveAsync();

                await message.AddReactionsAsync(emotes.ToArray());

                await FollowupAsync("Reaction Roles have been set up.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await FollowupAsync("An error occurred. Command failed.");
                await Context.DelayDeleteOriginalAsync();
            }
        }

        [SlashCommand("add_reaction_roles", "Edit the reaction roles embed message to add a new role.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EditReactionRoles([Summary(description: "Select an emoji for the role")] string selectedEmoji, [Summary(description: "Select a role")] SocketRole role) {

            await DeferAsync();

            var channel = Context.Channel; 

            if (_settings.IDs?.ReactionRoleMessageId == null) {
                await FollowupAsync("Reaction roles message not found.", ephemeral: true);
                return;
            }

            var messageId = _settings.IDs.ReactionRoleMessageId;
            IUserMessage message = (IUserMessage)await channel.GetMessageAsync(messageId);

            //if (await channel.GetMessageAsync(messageId) is not IUserMessage message) {
            //    await FollowupAsync("Reaction roles message not found.", ephemeral: true);
            //    return;
            //}

            var embed = message.Embeds.FirstOrDefault();
            if (embed == null) {
                await FollowupAsync("No embed found in the reaction roles message.", ephemeral: true);
                return;
            }

            var newDescription = embed.Description + $"\n{selectedEmoji} - {role.Name}";

            var newEmbed = new EmbedBuilder()
                .WithTitle(embed.Title)
                .WithDescription(newDescription)
                .WithColor(embed.Color!.Value)
                .WithImageUrl(embed.Image?.Url)
                .Build();

            await message.ModifyAsync(msg => msg.Embed = newEmbed);

            // Try parsing emoji (can be custom emote or Unicode)
            IEmote emote;
            if (Emote.TryParse(selectedEmoji, out var parsedEmote)) {
                emote = parsedEmote;
            } else {
                emote = new Emoji(selectedEmoji);
            }

            await message.AddReactionAsync(emote);

            // Register the new reaction role in handler and save to JSON
            await _reactionRoleHandler.AddReactionRoleAsync(selectedEmoji, role.Id);

            _settings.ReactionRoles.Add(new ReactionRoleEntry { Emoji = selectedEmoji, RoleId = role.Id });
            await _settingsService.SaveAsync();

            await FollowupAsync("Reaction role has been added and embed updated.", ephemeral: true);
        }


        [SlashCommand("add_bot_reaction", "Use this command to ONLY add a new reaction, to setup a reaction role use add_reaction_roles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddReactionRole(
            [Summary(description: "Select an emoji for the role")] string selectedEmoji,
            [Summary(description: "Select a role")] SocketRole role
        ) {
            await DeferAsync();

            try {
                if (_settings.IDs?.ReactionRoleMessageId == null) {
                    Console.WriteLine("[ReactionRoleHandler] Reaction roles message not found.");
                    return;
                }

                var channel = Context.Channel;
                var messageId = _settings.IDs.ReactionRoleMessageId;
                var message = await channel.GetMessageAsync(messageId) as IUserMessage;

                if (message == null) {
                    await FollowupAsync("Reaction role message not found.", ephemeral: true);
                    return;
                }

                // Add the reaction to the message
                var emote = new Emoji(selectedEmoji);
                await message.AddReactionAsync(emote);

                // Register the new reaction role
                await _reactionRoleHandler.AddReactionRoleAsync(selectedEmoji, role.Id);

                // Save to settings
                _settings.ReactionRoles.Add(new ReactionRoleEntry {
                    Emoji = selectedEmoji,
                    RoleId = role.Id
                });

                await _settingsService.SaveAsync();

                await FollowupAsync("Reaction has been added and saved.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                await FollowupAsync($"An error has occurred: {ex.Message}");
                await Context.DelayDeleteOriginalAsync();
            }
        }


    }
}
