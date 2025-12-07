using Lyuze.Core.Configuration;

namespace Lyuze.Core.Services.Interfaces {
    public interface ISettingsService {
        /// <summary>
        /// The currently loaded configuration.
        /// </summary>
        SettingsConfig Value { get; }

        /// <summary>
        /// Reload settings from disk into <see cref="Value"/>.
        /// </summary>
        Task ReloadAsync();

        /// <summary>
        /// Save the current <see cref="Value"/> to disk.
        /// </summary>
        Task SaveAsync();
    }
}

