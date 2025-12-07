using Discord;
using Discord.Interactions;
using Lyuze.Core.Configuration;
using Lyuze.Core.Services;

namespace Lyuze.Core.Utilities {
    public class MasterUtilities(SettingsConfig settings, ReactionRolesService reactionRoles) {
        private readonly SettingsConfig _settings = settings;
        private readonly ReactionRolesService _reactionRoles = reactionRoles;
        private readonly Random _rand = new();

        private static readonly string[] DefaultStatus = [
            "Online",
            "Idle",
            "Do Not Disturb"
        ];

        /// <summary>
        /// Status list from settings, with a fallback if empty.
        /// </summary>
        public IReadOnlyList<string> StatusList =>
            _settings.Status is { Count: > 0 } ? _settings.Status : DefaultStatus;

        /// <summary>
        /// Returns a random valid index for the given list.
        /// </summary>
        public int RandomListIndex(IReadOnlyList<string> list) {
            if (list == null || list.Count == 0)
                return 0;

            return _rand.Next(list.Count);
        }

        /// <summary>
        /// Initializes reaction roles from settings into the ReactionRolesService.
        /// </summary>
        public async Task InitializeReactionRolesAsync() {
            var reactionRoles = _settings.ReactionRoles;

            if (reactionRoles == null || reactionRoles.Count == 0) {
                Console.WriteLine("[ReactionRoles] No reaction roles found in settings.");
                return;
            }

            foreach (var rr in reactionRoles) {
                await _reactionRoles.AddReactionRoleAsync(rr.Emoji, rr.RoleId);
            }

            Console.WriteLine($"[ReactionRoles] Loaded {reactionRoles.Count} reaction roles from JSON.");
        }

        /// <summary>
        /// Returns a random color as a uint, based on predefined hex codes.
        /// </summary>
        public uint RandomEmbedColor() {
            var colors = new[] {
                "DC143C", // Crimson
                "C3E4E8", // Light Cyan
                "FF5733", // Light Orange
                "E6E6FA", // Lavender
                "7289DA", // Discord Purple
                "5865F2", // Discord Blurple
                "D2042D", // Cherry Red
                "8DB600", // Apple Green
                "87CEEB"  // Sky Blue
            };

            var hex = colors[_rand.Next(colors.Length)];

            // Parse as HEX, not decimal
            return Convert.ToUInt32(hex, 16);
        }

        /// <summary>
        /// Delay and delete the original interaction response.
        /// Pure helper, so static is fine.
        /// </summary>
        public static async Task DelayAndDeleteResponseAsync(IInteractionContext context, int delaySeconds = 5) {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            await context.Interaction.DeleteOriginalResponseAsync();
        }
    }
}
