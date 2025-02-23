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
    }
}
