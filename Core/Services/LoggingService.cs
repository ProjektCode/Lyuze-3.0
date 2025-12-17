using Discord;
using Lyuze.Core.Services.Interfaces;
using Pastel;
using System.Text;

namespace Lyuze.Core.Services {
    public class LoggingService : ILoggingService {
        private readonly string _basePath;
        private readonly string _logPath;

        // Lock object for thread safety on console writes and log async calls
        private readonly object _consoleLock = new();

        public LoggingService() {
            _basePath = AppDomain.CurrentDomain.BaseDirectory;
            _logPath = Path.Combine(_basePath, "Resources", "Logging");
            Directory.CreateDirectory(_logPath);
        }

        public async Task LogAsync(string source, LogSeverity severity, string message, Exception? exception = null) {
            // Synchronize concurrent logging calls
            lock (_consoleLock) {
                var sb = new StringBuilder();

                // Severity string colored with Pastel
                sb.Append($"{GetSeverityString(severity)}".Pastel(GetPastelHexColor(severity)));

                // Source string colored with Pastel
                sb.Append($" [{SourceToString(source)}] ".Pastel("#90EE72"));

                // Message or exception text (no color to avoid mixing with Pastel)
                if (!string.IsNullOrWhiteSpace(message))
                    sb.Append(message);
                else if (exception == null)
                    sb.Append("Unknown Exception. Exception Returned Null.");
                else if (exception.Message == null)
                    sb.Append($"Unknown {exception.StackTrace}");
                else
                    sb.Append(exception.Message);

                // Append a single newline at the end
                sb.Append('\n');

                // Write to console
                Console.Write(sb.ToString());
            }

            // If you want to add file logging or other async tasks here, you can do that outside the lock.
            await Task.CompletedTask;
        }

        public Task LogCriticalAsync(string source, string message, Exception? exception = null) =>
            LogAsync(source, LogSeverity.Critical, message, exception);

        public Task LogInformationAsync(string source, string message) =>
            LogAsync(source, LogSeverity.Info, message);

        public Task LogSetupAsync(string source, string message) =>
            LogAsync(source, LogSeverity.Verbose, message);

        public Task LogWarningAsync(string source, string message) =>
            LogAsync(source, LogSeverity.Warning, message);

        public Task LogErrorAsync(string source, string message, Exception exception) =>
            LogAsync(source, LogSeverity.Error, message, exception);

        private static string SourceToString(string src) {
            if (string.IsNullOrWhiteSpace(src)) return "UNKWN";

            return src.ToLower() switch {
                "discord" => "DISCD",
                "audio" => "AUDIO",
                "admin" => "ADMIN",
                "gateway" => "GTWAY",
                "blacklist" => "BLAKL",
                "bot" => "BOTWN",
                "setup" => "SETUP",
                "command" => "CMMND",
                "database" => "DBASE",
                "roles" => "ROLES",
                "jikan" => "JIKAN",
                "image" => "IMAGE",
                "fun" => "FUNCS",
                "general" => "GENRL",
                "backgrounds" => "BACKG",
                "gifs" => "GIFSS",
                "deleted" => "DELET",
                "join" => "JOINN",
                "profile" => "PRFIL",
                "waifu" => "WAIFU",
                "reaction-roles" => "RROLS",
                _ => src
            };
        }

        private static string GetSeverityString(LogSeverity severity) => severity switch {
            LogSeverity.Critical => "CRIT",
            LogSeverity.Debug => "DBUG",
            LogSeverity.Error => "EROR",
            LogSeverity.Info => "INFO",
            LogSeverity.Verbose => "VERB",
            LogSeverity.Warning => "WARN",
            _ => "UNKN"
        };

        private static string GetPastelHexColor(LogSeverity severity) => severity switch {
            LogSeverity.Critical => "#FF0000", // Red
            LogSeverity.Debug => "#FF00FF", // Magenta
            LogSeverity.Error => "#8B0000", // DarkRed
            LogSeverity.Info => "#90EE72", // LightGreen (same as your source pastel green)
            LogSeverity.Verbose => "#8B008B", // DarkMagenta
            LogSeverity.Warning => "#FFFF00", // Yellow
            _ => "#FFFFFF" // White
        };

    }
}
