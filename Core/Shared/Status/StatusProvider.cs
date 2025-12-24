using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Infrastructure.Configuration;

namespace Lyuze.Core.Shared.Status {
    public class StatusProvider(SettingsConfig settings) : IStatusProvider {
        private readonly SettingsConfig _settings = settings;
        
        private static readonly string[] DefaultStatuses = [
            "Online",
            "Idle",
            "Do Not Disturb",
            "Invisible",
            "Away",
            "Offline",
            "Bot Available at Github.com/ProjektCode"
        ];

        public IReadOnlyList<string> GetStatuses() => _settings.Status is { Count: > 0 } ? _settings.Status : DefaultStatuses;

        public int GetRandomStatusIndex(IReadOnlyList<string> list) => list is { Count: > 0 } ? Random.Shared.Next(list.Count) : 0;
    }
}
