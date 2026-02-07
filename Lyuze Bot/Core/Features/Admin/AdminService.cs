using Discord;
using Discord.WebSocket;

namespace Lyuze.Core.Features.Admin;

/// <summary>
/// Service responsible for administrative operations like message purging.
/// This separates business logic from the interaction module.
/// </summary>
public sealed class AdminService {

    /// <summary>
    /// Fetches messages from a channel that are within the 14-day bulk delete window.
    /// </summary>
    /// <param name="channel">The text channel to fetch messages from.</param>
    /// <param name="amount">Maximum number of messages to fetch.</param>
    /// <returns>A result containing the messages to delete and any status information.</returns>
    public static async Task<PurgeResult> GetMessagesToDeleteAsync(ITextChannel channel, int amount) {
        if (amount <= 0) {
            return PurgeResult.Failure("Amount must be greater than 0.");
        }

        if (amount > 1000) {
            return PurgeResult.Failure("Amount must be 1000 or less.");
        }

        var cutoff = DateTimeOffset.UtcNow.AddDays(-14);
        var toDelete = new List<IMessage>(capacity: Math.Min(amount, 1000));
        ulong? beforeId = null;

        while (toDelete.Count < amount) {
            var remaining = amount - toDelete.Count;
            var pageSize = Math.Min(100, remaining);

            IEnumerable<IMessage> page;

            if (beforeId.HasValue) {
                page = await channel
                    .GetMessagesAsync(beforeId.Value, Direction.Before, pageSize)
                    .FlattenAsync();
            } else {
                page = await channel
                    .GetMessagesAsync(pageSize)
                    .FlattenAsync();
            }

            var pageList = page.ToList();
            if (pageList.Count == 0)
                break;

            foreach (var msg in pageList) {
                if (msg.Timestamp < cutoff) {
                    // Past the 14-day cutoff, stop processing
                    beforeId = null;
                    break;
                }

                if (msg is IUserMessage userMsg) {
                    toDelete.Add(userMsg);
                }
            }

            beforeId = pageList.Last().Id;

            if (pageList.Last().Timestamp < cutoff)
                break;
        }

        if (toDelete.Count == 0) {
            return PurgeResult.Failure("Nothing to delete (within 14 days).");
        }

        return PurgeResult.Success(toDelete);
    }

    public static async Task<BanResult> BanUserAsync(IGuild guild, SocketGuildUser user, int pruneDays, string? reason) {

        
        if (user is null) {
            return BanResult.Failure("User not found in the guild.");
        }
        await guild.AddBanAsync(user, pruneDays, reason ?? "No reason given", new RequestOptions { AuditLogReason = reason ?? "No reason given." });
        return BanResult.Success(user.Username, reason ?? "No reason given.");
    }

    public static async Task<UnBanResult> UnbanUserAsync(IGuild guild, string userIdString, string? reason) {

        if (!ulong.TryParse(userIdString, out var userId)) {
            return UnBanResult.Failure("Invalid user ID format.");
        }

        var ban = await guild.GetBanAsync(userId);
        if (ban is null) {
            return UnBanResult.Failure("User is not banned.");
        }

        await guild.RemoveBanAsync(userId, new RequestOptions { AuditLogReason = reason ?? "No reason given." });

        return UnBanResult.Success(ban.User.Username);
    }

    ///<summary>
    /// Gets a list of banned users in the guild, up to the specified limit.
    ///</summary>
    public static async Task<IReadOnlyList<IBan>> GetBannedUsersAsync(IGuild guild, int limit = 25) {
        var bans = await guild.GetBansAsync(limit).FlattenAsync();
        return bans.ToList();
    }


    /// <summary>
    /// Deletes the provided messages from the channel.
    /// </summary>
    public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IReadOnlyCollection<IMessage> messages) {
        await channel.DeleteMessagesAsync(messages);
        return messages.Count;
    }
}

/// <summary>
/// Represents the result of a purge operation.
/// </summary>
public readonly record struct PurgeResult {
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<IMessage>? Messages { get; init; }

    public static PurgeResult Success(IReadOnlyList<IMessage> messages) =>
        new() { IsSuccess = true, Messages = messages };

    public static PurgeResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Represents the result of a ban operation.
/// </summary>
public readonly record struct BanResult {
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Username { get; init; }
    public string? Reason { get; init; }
    public static BanResult Success(string name, string reason) =>
        new() { IsSuccess = true, Username = name, Reason = reason };
    public static BanResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage};
}


/// <summary>
/// Represents the result of an unban operation.
/// </summary>
public readonly record struct UnBanResult {
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Username { get; init; }
    public static UnBanResult Success(string name) =>
        new() { IsSuccess = true, Username = name };
    public static UnBanResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage};
}
