using Discord;

namespace Lyuze.Core.Services.Interfaces {
    public interface ILoggingService {
        Task LogAsync(string source, LogSeverity severity, string message, Exception? exception = null);

        Task LogCriticalAsync(string source, string message, Exception? exception = null);

        Task LogInformationAsync(string source, string message);

        Task LogWarningAsync(string source, string message);
        Task LogErrorAsync(string message, Exception exception, string source);

        Task LogSetupAsync(string source, string message);
    }
}
