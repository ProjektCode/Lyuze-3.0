using Discord.Interactions;
using Lyuze.Core.Handlers;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Lyuze.Core.Services {
    public class N8nService(ILoggingService logger, SettingsHandler settings) {
        private readonly HttpClient _httpClient = new();
        private readonly ILoggingService _logger = logger;
        private readonly SettingsHandler _settings = settings;

        /// <summary>
        /// Sends an anime tracking payload to n8n.
        /// </summary>
        public async Task<bool> SendAnimeTrackingAsync(int id, ulong ChannelID, bool deleteRow = false) {
            try {
                string webhookUrlString = _settings.n8n?.WebhookUrl ?? string.Empty;

                if (string.IsNullOrWhiteSpace(webhookUrlString) ||
                    !Uri.TryCreate(webhookUrlString, UriKind.Absolute, out var webhookUri)) {
                    await _logger.LogCriticalAsync("n8n", $"Invalid or missing Webhook URL: {webhookUrlString ?? "null"}");
                    return false;
                }

                var payload = new {
                    id,
                    latestEpisode = 0,
                    deleteRow,
                    channelID = ChannelID,//This is not returning the actual ChannelID where the command was sent from. Need to figure out how to get that.
                };

                var json = JsonSerializer.Serialize(payload);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUri, content);

                if (response.IsSuccessStatusCode) {
                    await _logger.LogInformationAsync("n8n", $"Successfully sent anime ID {id} to n8n webhook.");
                    return true;
                } else {
                    await _logger.LogCriticalAsync("n8n", $"Failed to send ID {id}. HTTP {(int)response.StatusCode}");
                    return false;
                }
            } catch (Exception ex) {
                await _logger.LogCriticalAsync("n8n", $"Exception in SendAnimeTrackingAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendActionAsync(string action, ulong ChannelID) {
            try {
                string webhookUrlString = _settings.n8n?.WebhookUrl ?? string.Empty;

                if (string.IsNullOrWhiteSpace(webhookUrlString) ||
                    !Uri.TryCreate(webhookUrlString, UriKind.Absolute, out var webhookUri)) {
                    await _logger.LogCriticalAsync("n8n", $"Invalid or missing Webhook URL: {webhookUrlString ?? "null"}");
                    return false;
                }
                await _logger.LogInformationAsync("n8n", webhookUri.ToString());
                var payload = new {
                    action,
                    channelID = ChannelID,
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUri, content);

                if (response.IsSuccessStatusCode) {
                    await _logger.LogInformationAsync("n8n", $"Successfully sent action {action} to n8n webhook.");
                    return true;
                } else {
                    await _logger.LogCriticalAsync("n8n", $"Failed to send action {action}. HTTP {(int)response.StatusCode}");
                    return false;
                }


            } catch (Exception ex){
                await _logger.LogCriticalAsync("n8n", $"Exception in SendActionAsync: {ex.Message}");
                return false;
            }}
    }
}
