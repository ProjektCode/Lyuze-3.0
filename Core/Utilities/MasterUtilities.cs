using Discord;
using Lyuze.Core.Handlers;

namespace Lyuze.Core.Utilities {
    public class MasterUtilities {
        private readonly Random rand = new();
        public List<String> sList = SettingsHandler.Instance.Status;

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

        public static void ReactionRoles(ReactionRoleHandler _reactionRoleHandler) { //EmojiID, RoleID

            try {

                _reactionRoleHandler.AddReactionRole("<:GachaUpdates:1343843757623349369>", 1343845467901132875); //Gacha
                _reactionRoleHandler.AddReactionRole("<:GameUpdates:1343843759540015104>", 1343837048263675964); //Game Updates
                _reactionRoleHandler.AddReactionRole("<:Weeb:1343843763105038457>", 758185141775892511); //Weeb

                _reactionRoleHandler.AddReactionRole("❤️", 1343862327442931802); //Red Name
                _reactionRoleHandler.AddReactionRole("💙", 1343862681169563711); //Blue Name
                _reactionRoleHandler.AddReactionRole("💜", 1343862754943041586); //Purple Name
                _reactionRoleHandler.AddReactionRole("🖤", 1343862900988575784); //Black Name
                _reactionRoleHandler.AddReactionRole("💛", 1343860522994630656); //Yellow Name
                _reactionRoleHandler.AddReactionRole("💚", 1343863270745968640); //Green Name
                _reactionRoleHandler.AddReactionRole("🧡", 1343863348743241728); //Orange Name
                _reactionRoleHandler.AddReactionRole("🤎", 1343863433342353418); //Brown Name
                _reactionRoleHandler.AddReactionRole("🤍", 1343863504540798987); //White Name
                _reactionRoleHandler.AddReactionRole("🩷", 1343863591962546327); //Pink Name
                _reactionRoleHandler.AddReactionRole("🩶", 1343863724552880199); //Grey Name
                _reactionRoleHandler.AddReactionRole("🩵", 1343863816240496691); //Light Blue Name
                _reactionRoleHandler.AddReactionRole("🌈", 1343897392293875753); //Rainbow Name

            } catch (Exception ex) { Console.WriteLine(ex.ToString()); };

        }

    }
}
