using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Features.Admin.Components;

/// <summary>
/// Handles the SelectMenu interaction when a moderator picks a banned user to unban.
/// </summary>
public class UnbanSelectMenuHandler(AdminService adminService, ILoggingService logger)
    : InteractionModuleBase<SocketInteractionContext> {

    private readonly AdminService _adminService = adminService;
    private readonly ILoggingService _logger = logger;

    /// <summary>
    /// Handles the unban dropdown selection.
    /// CustomId format: "unban-select:{moderatorId}"
    /// </summary>
    /// <param name="moderatorId">The ID of the mod who invoked /unban (embedded in customId).</param>
    /// <param name="selections">The selected values (we only allow 1).</param>
    [ComponentInteraction("unban-select:*")]
    public async Task HandleUnbanSelection(string moderatorId, string[] selections) {
        // 1. Security check: Only the original moderator can use this dropdown
        if (Context.User.Id.ToString() != moderatorId) {
            await RespondAsync(
                "You cannot use this dropdown. Only the moderator who ran `/unban` can select.",
                ephemeral: true
            );
            return;
        }

        // 2. Defer the response (we'll update the original message)
        await DeferAsync(ephemeral: true);

        // 3. Get the selected userId
        var selectedUserId = selections.FirstOrDefault();
        if (string.IsNullOrEmpty(selectedUserId)) {
            await FollowupAsync("No user selected.", ephemeral: true);
            return;
        }

        // 4. Perform the unban
        var result = await AdminService.UnbanUserAsync(Context.Guild, selectedUserId, reason: null);

        // 5. Build the response
        if (!result.IsSuccess) {
            await FollowupAsync($"Failed to unban: {result.ErrorMessage}", ephemeral: true);
            return;
        }

        // 6. Update the original message to show completion and disable the dropdown
        var disabledMenu = new SelectMenuBuilder()
            .WithCustomId($"unban-select:{moderatorId}")
            .WithPlaceholder($"Unbanned: {result.Username}")
            .WithDisabled(true)
            .WithOptions(new List<SelectMenuOptionBuilder> {
                new SelectMenuOptionBuilder()
                    .WithLabel("Completed")
                    .WithValue("done")
            });

        var component = new ComponentBuilder()
            .WithSelectMenu(disabledMenu)
            .Build();

        var embed = new EmbedBuilder()
            .WithTitle("User Unbanned")
            .WithDescription($"Successfully unbanned **{result.Username}** (ID: {selectedUserId})")
            .WithColor(Color.Green)
            .Build();

        // ModifyOriginalResponseAsync updates the message the dropdown was attached to
        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = component;
        });
    }
}