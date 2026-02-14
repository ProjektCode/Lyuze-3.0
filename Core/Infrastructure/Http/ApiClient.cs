using System.Net.Http;
using Lyuze.Core.Abstractions.Interfaces;

namespace Lyuze.Core.Infrastructure.Http;

public sealed class ApiClient(ILoggingService logger, HttpClient httpClient) : IApiClient {
    private readonly ILoggingService _logger = logger;
    private readonly HttpClient _http = httpClient;

    private static string SanitizeUrlForLogs(string url) {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
            return url;
        }

        // Never log full secrets or huge query values.
        if (string.IsNullOrWhiteSpace(uri.Query)) {
            return uri.GetLeftPart(UriPartial.Path);
        }

        static bool IsSensitiveKey(string key) {
            key = key.Trim().ToLowerInvariant();
            return key is "api_key" or "apikey" or "token" or "key" or "authorization" or "password" or "login";
        }

        var pairs = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => {
                var idx = p.IndexOf('=');
                var k = idx >= 0 ? p[..idx] : p;
                var v = idx >= 0 ? p[(idx + 1)..] : string.Empty;
                if (IsSensitiveKey(k) || k.Equals("search[url]", StringComparison.OrdinalIgnoreCase)) {
                    v = "<redacted>";
                }
                return (Key: k, Value: v);
            })
            .Take(12)
            .ToList();

        var query = string.Join("&", pairs.Select(x => string.IsNullOrWhiteSpace(x.Value) ? x.Key : $"{x.Key}={x.Value}"));
        return uri.GetLeftPart(UriPartial.Path) + "?" + query;
    }

    public async Task<T?> GetJsonAsync<T>(string source, string url, Func<string, T?> deserialize, CancellationToken ct = default) {

        try {
            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                var safeUrl = SanitizeUrlForLogs(url);
                var body = string.Empty;
                try {
                    body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                } catch {
                    // ignore - best effort diagnostics
                }

                if (body.Length > 400) {
                    body = body[..400] + "...";
                }

                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {safeUrl}{(string.IsNullOrWhiteSpace(body) ? "" : $" | Body: {body}")}");
                return default;
            }

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return deserialize(json);
        } catch (OperationCanceledException) {
            await _logger.LogWarningAsync(source, $"Request cancelled: {SanitizeUrlForLogs(url)}");
            return default;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetJsonAsync for {SanitizeUrlForLogs(url)}", ex);
            return default;
        }

    }

    public async Task<byte[]?> GetBytesAsync(string source, string url, CancellationToken ct = default) {

        try {
            using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {SanitizeUrlForLogs(url)}");
                return null;
            }

            return await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        } catch (OperationCanceledException) {
            await _logger.LogWarningAsync(source, $"Request cancelled: {SanitizeUrlForLogs(url)}");
            return null;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetBytesAsync for {SanitizeUrlForLogs(url)}", ex);
            return null;
        }

    }

    public async Task<Stream?> GetStreamAsync(string source, string url, CancellationToken ct = default) {
        try {
            var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                resp.Dispose();
                await _logger.LogWarningAsync(source, $"HTTP {(int)resp.StatusCode} on GET {SanitizeUrlForLogs(url)}");
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
            await _logger.LogWarningAsync(source, $"Request cancelled: {SanitizeUrlForLogs(url)}");
            return null;
        } catch (Exception ex) {
            await _logger.LogErrorAsync(source, $"Exception in GetStreamAsync for {SanitizeUrlForLogs(url)}", ex);
            return null;
        }
    }
}
