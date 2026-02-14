using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Lyuze.Core.Abstractions.Interfaces;

public interface IEmbedService {
    Task<Embed> ErrorEmbedAsync(string source, string error);
    Task<Embed> ProfileEmbedAsync(SocketGuildUser user, SocketInteractionContext ctx);
    Task<Embed> UpdatedProfileAsync(SocketGuildUser user, SocketInteractionContext ctx, string updatedSection, string entry);
}
