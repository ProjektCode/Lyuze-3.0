using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Features.Profiles {
    public class ProfileService(IEmbedService embedService, IPlayerService playerService, ILoggingService loggingService) {
        private readonly IEmbedService _embedService = embedService;
        private readonly IPlayerService _playerService = playerService;
        private readonly ILoggingService _loggingService = loggingService;

        /// <summary>
        /// Validates that the caller is either the target user or has admin permissions.
        /// </summary>
        private async Task<Embed?> ValidateOwnerOrAdminAsync(SocketGuildUser user, SocketInteractionContext ctx) {
            var caller = (SocketGuildUser)ctx.User;
            if (user.Id != caller.Id && !caller.GuildPermissions.BanMembers)
                return await _embedService.ErrorEmbedAsync("ProfileService", "You can only update your own profile.");
            return null;
        }

        public async Task<Embed> GetProfileAsync(SocketGuildUser user, SocketInteractionContext ctx) {
            try {
                var player = await _playerService.GetUserAsync(user);
                if (!player.PublicProfile) 
                    return await _embedService.ErrorEmbedAsync("ProfileService", "This user's profile is private");

                return await _embedService.ProfileEmbedAsync(user, ctx);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("profile", $"Error getting profile for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }
        }

        public async Task<Embed> UpdateProfileBackgroundAsync(SocketGuildUser user, SocketInteractionContext ctx, string backgroundUrl) {
            try {
                var error = await ValidateOwnerOrAdminAsync(user, ctx);
                if (error != null) return error;

                var player = await _playerService.GetUserAsync(user);
                player.Background = backgroundUrl;
                await _playerService.UpdateUserAsync(user, player);

                return await _embedService.UpdatedProfileAsync(user, ctx, "Background", backgroundUrl);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("profile", $"Error updating profile for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }
        }

        public async Task<Embed> UpdateProfileAboutMeAsync(SocketGuildUser user, SocketInteractionContext ctx, string aboutMe) {
            try {
                var error = await ValidateOwnerOrAdminAsync(user, ctx);
                if (error != null) return error;

                var player = await _playerService.GetUserAsync(user);
                player.AboutMe = aboutMe;
                await _playerService.UpdateUserAsync(user, player);

                return await _embedService.UpdatedProfileAsync(user, ctx, "About Me", aboutMe);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("profile", $"Error updating about me for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }
        }

        public async Task<Embed> UpdateProfilePublicAsync(SocketGuildUser user, SocketInteractionContext ctx, bool isPublic) {
            try {
                var error = await ValidateOwnerOrAdminAsync(user, ctx);
                if (error != null) return error;

                var player = await _playerService.GetUserAsync(user);
                player.PublicProfile = isPublic;
                await _playerService.UpdateUserAsync(user, player);

                string status = player.PublicProfile ? "Public" : "Private";
                return await _embedService.UpdatedProfileAsync(user, ctx, "Profile Visibility", status);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("profile", $"Error updating profile status for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }
        }

        public async Task<Embed> UpdateLevelNotificationsAsync(SocketGuildUser user, SocketInteractionContext ctx) {
            try {
                var error = await ValidateOwnerOrAdminAsync(user, ctx);
                if (error != null) return error;

                var player = await _playerService.GetUserAsync(user);
                player.LevelNotify = !player.LevelNotify;
                await _playerService.UpdateUserAsync(user, player);

                string status = player.LevelNotify ? "Enabled" : "Disabled";
                return await _embedService.UpdatedProfileAsync(user, ctx, "Level Notifications", status);
            } catch (Exception ex) {
                await _loggingService.LogErrorAsync("profile", $"Error updating level notifications for {user.Username}: ", ex);
                return await _embedService.ErrorEmbedAsync("ProfileService", ex.Message);
            }
        }
    }
}
