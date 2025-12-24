namespace Lyuze.Core.Abstractions.Interfaces {
    public interface IApiClient {
        Task<T?> GetJsonAsync<T>(string source, string url, Func<string, T?> deserialize, CancellationToken ct = default);
    }
}
