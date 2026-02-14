using Lyuze.Core.Infrastructure.Configuration;

namespace Lyuze.Core.Abstractions.Interfaces {
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

