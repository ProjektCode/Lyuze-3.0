using Discord.WebSocket;
using Lyuze.Core.Database.Model;

namespace Lyuze.Core.Services.Interfaces {
    public interface IPlayerService {
        Task CreateProfileAsync(SocketGuildUser user);
        Task<bool> HasProfileAsync(SocketGuildUser user);
        Task<PlayerModel> GetUserAsync(SocketGuildUser user);
        Task UpdateUserAsync(SocketGuildUser user, PlayerModel player);
    }
}