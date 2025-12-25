using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Shared.Embeds;

namespace Lyuze.Core.Features.Profiles {
    public class ProfileService(EmbedService embedService, IPlayerService playerService, ILoggingService loggingService) {
        public readonly EmbedService _embedService = embedService;
        public readonly IPlayerService _playerService = playerService;
        public readonly ILoggingService _loggingService = loggingService;

        public async Task<Embed> GetProfileAsync(SocketGuildUser user, SocketInteractionContext ctx) {

            try {

                var player = await _playerService.GetUserAsync(user);
                if (!player.PublicProfile) return await _embedService.ErrorEmbedAsync("ProfileService", "This user's profile is private");

                return await _embedService.ProfileEmbedAsync(user, ctx);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("ProfileService", $"Error getting profile for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }

        }

        public async Task<Embed> UpdateProfileBackgroundAsync(SocketGuildUser user, SocketInteractionContext ctx, string backgroundUrl) {

            try {

                var player = await _playerService.GetUserAsync(user);

                if(user.Id != ctx.User.Id || !user.GuildPermissions.BanMembers) return await _embedService.ErrorEmbedAsync("ProfileService", "You can only update your own profile background.");

                player.Background = backgroundUrl;
                await _playerService.UpdateUserAsync(user, player);

                return await _embedService.UpdatedProfileAsync(user, ctx, "Background", backgroundUrl);

            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("ProfileService", $"Error updating profile for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }

        }

        public async Task<Embed> UpdateProfileAboutMeAsync(SocketGuildUser user, SocketInteractionContext ctx, string aboutMe) {

            try {
                var player = await _playerService.GetUserAsync(user);

                if (user.Id != ctx.User.Id || !user.GuildPermissions.BanMembers) return await _embedService.ErrorEmbedAsync("ProfileService", "You can only update your own profile background.");

                player.AboutMe = aboutMe;
                await _playerService.UpdateUserAsync(user, player);
                return await _embedService.UpdatedProfileAsync(user, ctx, "About Me", aboutMe);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("ProfileService", $"Error updating about me for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }

        }

        public async Task<Embed> UpdateProfilePublicAsync(SocketGuildUser user, SocketInteractionContext ctx, bool isPublic) {

            try {
                var player = await _playerService.GetUserAsync(user);

                if (user.Id != ctx.User.Id || !user.GuildPermissions.BanMembers) return await _embedService.ErrorEmbedAsync("ProfileService", "You can only update your own profile background.");

                player.PublicProfile = isPublic;
                await _playerService.UpdateUserAsync(user, player);
                string status = player.PublicProfile ? "Public" : "Private";
                return await _embedService.UpdatedProfileAsync(user, ctx, "Profile Visibility", status);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("ProfileService", $"Error updating profile status for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }

        }

        public async Task<Embed> UpdateLevelNotificationsAsync(SocketGuildUser user, SocketInteractionContext ctx) {

            try {
                var player = await _playerService.GetUserAsync(user);

                if (user.Id != ctx.User.Id || !user.GuildPermissions.BanMembers) return await _embedService.ErrorEmbedAsync("ProfileService", "You can only update your own profile background.");

                player.LevelNotify = !player.LevelNotify;
                await _playerService.UpdateUserAsync(user, player);
                string status = player.LevelNotify ? "Enabled" : "Disabled";
                return await _embedService.UpdatedProfileAsync(user, ctx, "Level Notifications", status);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("ProfileService", $"Error updating level notifications for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }

        }

    }
}
