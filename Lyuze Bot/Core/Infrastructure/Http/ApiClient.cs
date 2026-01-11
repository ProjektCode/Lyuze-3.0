using System.Net.Http;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Infrastructure.Http;

public sealed class ApiClient(ILoggingService logger, HttpClient httpClient) : IApiClient {
    private readonly ILoggingService _logger = logger;
    private readonly HttpClient _http = httpClient;

    public async Task<T?> GetJsonAsync<T>(string source, string url, Func<string, T?> deserialize, CancellationToken ct = default) {

        try {
            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {url}");
                return default;
            }

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return deserialize(json);
        } catch (OperationCanceledException) {
            await _logger.LogWarningAsync(source, $"Request cancelled: {url}");
            return default;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetJsonAsync for {url}", ex);
            return default;
        }

    }

    public async Task<byte[]?> GetBytesAsync(string source, string url, CancellationToken ct = default) {

        try {
            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {url}");
                return null;
            }

            return await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        } catch (OperationCanceledException) {
            await _logger.LogWarningAsync(source, $"Request cancelled: {url}");
            return null;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetBytesAsync for {url}", ex);
            return null;
        }

    }

    public async Task<Stream?> GetStreamAsync(string source, string url, CancellationToken ct = default) {
        try {
            var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                resp.Dispose();
                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {url}");
                return null;
            }

            // Caller must dispose the stream (which disposes response content stream),
            // BUT we must keep response alive; easiest is to copy to MemoryStream:
            var ms = new MemoryStream();
            await resp.Content.CopyToAsync(ms, ct).ConfigureAwait(false);
            resp.Dispose();
            ms.Position = 0;
            return ms;
        } catch (OperationCanceledException) {
            await _logger.LogWarningAsync(source, $"Request cancelled: {url}");
            return null;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetStreamAsync for {url}", ex);
            return null;
        }
    }
}
