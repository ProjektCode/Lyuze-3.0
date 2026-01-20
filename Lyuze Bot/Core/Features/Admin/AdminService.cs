using Discord;

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
    public async Task<PurgeResult> GetMessagesToDeleteAsync(ITextChannel channel, int amount) {
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

    /// <summary>
    /// Deletes the provided messages from the channel.
    /// </summary>
    public async Task<int> DeleteMessagesAsync(ITextChannel channel, IReadOnlyCollection<IMessage> messages) {
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
