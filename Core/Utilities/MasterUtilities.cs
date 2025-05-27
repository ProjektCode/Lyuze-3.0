using Discord;
using Lyuze.Core.Handlers;
using Lyuze.Core.Services;

namespace Lyuze.Core.Utilities {
    public class MasterUtilities {
        private readonly Random rand = new();
        public List<string> sList = SettingsHandler.Instance.Status ?? ["Online", "Idle", "Do Not Disturb"];


        // Static instance of the class
        private static readonly Lazy<MasterUtilities> _instance = new(() => new MasterUtilities());

        // Private constructor to prevent instantiation
        private MasterUtilities() { }

        // Public property to access the instance
        public static MasterUtilities Instance => _instance.Value;

        public int RandomListIndex(List<string> list) {
            var i = rand.Next(list.Count);
            return i;
        }

        // Helper method to delay and delete the original response
        public static async Task DelayAndDeleteResponseAsync(IInteractionContext context, int delaySeconds = 5) {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            await context.Interaction.DeleteOriginalResponseAsync();
        }
        public static void ReactionRoles(ReactionRolesService _reactionRoleHandler) {
            try {
                var reactionRoles = SettingsHandler.Instance.ReactionRoles;

                if (reactionRoles == null || reactionRoles.Count == 0) {
                    Console.WriteLine("[ReactionRoles] No reaction roles found in settings.");
                    return;
                }

                foreach (var rr in reactionRoles) {
                    _reactionRoleHandler.AddReactionRole(rr.Emoji, rr.RoleId);
                }

                Console.WriteLine($"[ReactionRoles] Loaded {reactionRoles.Count} reaction roles from JSON.");

            } catch (Exception ex) {
                Console.WriteLine($"[ReactionRoles] Exception: {ex}");
            }
        }


    }
}
