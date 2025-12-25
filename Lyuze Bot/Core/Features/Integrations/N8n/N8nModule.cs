using Discord.Interactions;
using Lyuze.Core.Extensions;

namespace Lyuze.Core.Features.Integrations.N8n {

    [Group("n8n", "Integration commands with the n8n automation system")]
    public class N8nModule(N8nService n8nService) : InteractionModuleBase<SocketInteractionContext> {
        private readonly N8nService _n8nService = n8nService;

        [SlashCommand("track", "Track a LiveChart anime ID")]
        public async Task TrackAsync(
            [Summary("id", "The LiveChart anime ID (e.g. 13202)")] int id) {

            await DeferAsync(ephemeral: true);

            bool success = await _n8nService.SendAnimeTrackingAsync(id, Context.Channel.Id, deleteRow: false);

            if (success) {
                await FollowupAsync($"✅ Sent anime ID `{id}` to n8n!", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            } else {
                await FollowupAsync($"⚠️ Failed to send anime ID `{id}` to n8n.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }
        }

        [SlashCommand("untrack", "Untrack a LiveChart anime ID")]
        public async Task UntrackAsync(
            [Summary("id", "The LiveChart anime ID (e.g. 13202)")] int id) {

            await DeferAsync(ephemeral: true);

            bool success = await _n8nService.SendAnimeTrackingAsync(id, Context.Channel.Id, deleteRow: true);

            if (success) {
                await FollowupAsync($"🗑️ Untracked anime ID `{id}` in n8n!", ephemeral: true);
                await Context.DelayDeleteOriginalAsync( );
            } else {
                await FollowupAsync($"⚠️ Failed to untrack anime ID `{id}`.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }
        }

        [SlashCommand("list", "Grab list of currently tracked IDs from Livechart.me")]
        public async Task LiskIDsAsync() {
            await DeferAsync(ephemeral: true);
            bool success = await _n8nService.SendActionAsync("list", Context.Channel.Id); //Figure out how to send the correct channel id where the command came from.
            if (success) {
                await FollowupAsync($"✅ Sent action `{"list"}` to n8n!", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            } else {
                await FollowupAsync($"⚠️ Failed to send action `{"list"}` to n8n.", ephemeral: true);
                await Context.DelayDeleteOriginalAsync();
            }

        }
    }
}
