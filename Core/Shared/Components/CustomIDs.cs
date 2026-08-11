namespace Lyuze.Core.Shared.Components {
    public static class CustomIds {
        public const string HelpCategoryPattern = "help-cat:*";
        public const string UnbanSelectPattern = "unban-select:*";

        public static string HelpCategory(ulong userId) => $"help-cat:{userId}";

        public static string UnbanSelect(ulong moderatorId) => $"unban-select:{moderatorId}";

        public static bool TryParseUserId(string value, out ulong userId) => ulong.TryParse(value, out userId);
    }
}