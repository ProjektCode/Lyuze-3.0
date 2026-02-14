using Discord.WebSocket;
using Lyuze.Core.Features.Profiles;

namespace Lyuze.Core.Abstractions.Interfaces {
    public interface IPlayerService {
        Task CreateProfileAsync(SocketGuildUser user);
        Task<bool> HasProfileAsync(SocketGuildUser user);
        Task<PlayerModel> GetUserAsync(SocketGuildUser user);
        Task UpdateUserAsync(SocketGuildUser user, PlayerModel player);
    }
}