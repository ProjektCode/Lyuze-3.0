using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Lyuze.Core.Modules {
    public class UserCommandsModule : InteractionModuleBase<SocketInteractionContext> {

        readonly ulong switchRole = 1223045733431775283;
        readonly ulong gamerRole = 760695943715749920;
        readonly ulong weebRole = 758185141775892511;


        [UserCommand("Add Switch Plays Role")]
        public async Task AddSwitchPlaysRole(IUser user) {
            await ((SocketGuildUser)user).AddRoleAsync(switchRole);

            await RespondAsync($"Role '{Context.Guild.GetRole(switchRole)}' has been added.");
        }

        [UserCommand("Add Gamer Role")]
        public async Task AddGamerRoleAsync(IUser user) {
            try {
                await ((SocketGuildUser)user).AddRoleAsync(gamerRole);
                await RespondAsync($"Role '{Context.Guild.GetRole(gamerRole)}' has been added.");
            } catch (Exception ex) { 
                await RespondAsync($"Adding role failed: {ex.Message}");
            }
        }

        [UserCommand("Add Weeb Role")]
        public async Task AddWeebRoleAsync(IUser user) {
            try {
                await ((SocketGuildUser)user).AddRoleAsync(weebRole);
                await RespondAsync($"Role '{Context.Guild.GetRole(weebRole)}' has been added.");
            } catch (Exception ex) { 
                await RespondAsync($"Adding role failed: {ex.Message}");
            }
        }

    }
}
