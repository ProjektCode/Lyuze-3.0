using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Utilities;

namespace Lyuze.Core.Modules {
    public class RolesModule : InteractionModuleBase<SocketInteractionContext> {

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

    }
}
