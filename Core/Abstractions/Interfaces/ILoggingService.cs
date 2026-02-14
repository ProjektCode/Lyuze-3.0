using Discord;

namespace Lyuze.Core.Abstractions.Interfaces {
    public interface ILoggingService {
        Task LogAsync(string source, LogSeverity severity, string message, Exception? exception = null);

        Task LogCriticalAsync(string source, string message, Exception? exception = null);

        Task LogInformationAsync(string source, string message);

        Task LogWarningAsync(string source, string message);
        Task LogErrorAsync(string source, string message, Exception exception);

        Task LogSetupAsync(string source, string message);
    }
}
