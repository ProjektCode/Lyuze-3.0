using Lyuze.Core.Services.Interfaces;

namespace Lyuze.Core.Services {
    public sealed class ApiClient(HttpClient http, ILoggingService logger) : IApiClient {
        private readonly HttpClient _http = http;
        private readonly ILoggingService _logger = logger;

        public async Task<T?> GetJsonAsync<T>(string source, string url, Func<string, T?> deserialize, CancellationToken ct = default) {

            try {
                using var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode) {
                    await _logger.LogWarningAsync(source, $"GET failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                    return default;
                }

                var content = await response.Content.ReadAsStringAsync(ct);

                if (string.IsNullOrWhiteSpace(content)) {
                    await _logger.LogWarningAsync(source, "Response body was empty.");
                    return default;
                }

                var model = deserialize(content);
                if (model == null) {
                    await _logger.LogWarningAsync(source, "Failed to deserialize response JSON.");
                    return default;
                }

                return model;
            } catch (OperationCanceledException) {
                await _logger.LogWarningAsync(source, "Request was canceled.");
                return default;
            } catch (Exception ex) {
                await _logger.LogErrorAsync(source, "Unexpected exception during GET JSON.", ex);
                return default;
            }

        }

    }

}
