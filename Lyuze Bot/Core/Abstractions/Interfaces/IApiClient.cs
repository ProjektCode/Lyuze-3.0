using System.Net.Http;

namespace Lyuze.Core.Abstractions.Interfaces;

public interface IApiClient {
    Task<T?> GetJsonAsync<T>(string source, string url, Func<string, T?> deserialize, CancellationToken ct = default);

    Task<byte[]?> GetBytesAsync(string source, string url, CancellationToken ct = default);

    Task<Stream?> GetStreamAsync(string source, string url, CancellationToken ct = default);
}

