using Discord;
using System;
using System.Threading.Tasks;

public interface ILoggingService {
    Task LogAsync(string source, LogSeverity severity, string message, Exception? exception = null);
    Task LogCriticalAsync(string source, string message, Exception? exception = null);
    Task LogInformationAsync(string source, string message);
    Task LogSetupAsync(string source, string message);
}
