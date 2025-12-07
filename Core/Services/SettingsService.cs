using Lyuze.Core.Configuration;
using Lyuze.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Lyuze.Core.Services {
    public class SettingsService : ISettingsService {
        private readonly string _filePath;

        public SettingsConfig Value { get; private set; }

        public SettingsService() {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _filePath = Path.Combine(basePath, "Resources", "Settings", "settings.json");

            Value = LoadFromDisk();
        }

        private SettingsConfig LoadFromDisk() {
            if (!File.Exists(_filePath)) {
                var settings = new SettingsConfig();
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.WriteAllText(_filePath, json);
                return settings;
            }

            var fileJson = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<SettingsConfig>(fileJson) ?? new SettingsConfig();
        }

        public Task ReloadAsync() {
            Value = LoadFromDisk();
            return Task.CompletedTask;
        }

        public async Task SaveAsync() {
            var json = JsonConvert.SerializeObject(Value, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
