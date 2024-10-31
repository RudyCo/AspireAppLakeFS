#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace AspireAppLakeFS.Client.Client;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A factory for creating <see cref="HttpClient"/> instances
/// </summary>
/// <param name="settings">
/// The <see cref="LakeFSClientSettings"/> settings for the server
/// </param>
public sealed class LakeFSClientFactory(LakeFSClientSettings settings) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private HttpClient? _client;

    /// <summary>
    /// Gets an <see cref="HttpClient"/> instance in the connected state
    /// (and that's been authenticated if configured).
    /// </summary>
    /// <param name="cancellationToken">Used to abort client creation and connection.</param>
    /// <returns>A connected (and authenticated) <see cref="HttpClient"/> instance.</returns>
    /// <remarks>
    /// Since both the connection and authentication are considered expensive operations,
    /// the <see cref="HttpClient"/> returned is intended to be used for the duration of a request
    /// (registered as 'Scoped') and is automatically disposed of.
    /// </remarks>
    public async Task<HttpClient> GetHttpClientAsync(
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            _client ??= new HttpClient() { BaseAddress = settings.Endpoint };
        }
        finally
        {
            _semaphore.Release();
        }

        return _client;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _semaphore.Dispose();
    }
}