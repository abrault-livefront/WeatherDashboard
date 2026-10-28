namespace WeatherDashboard.Infrastructure.IntegrationTests;

using Elastic.Clients.Elasticsearch;
using Testcontainers.Elasticsearch;

/// <summary>
///     Shares a single Elasticsearch container across the Elasticsearch-backed indexer and search service tests.
/// </summary>
public sealed class ElasticsearchContainerFixture : IAsyncLifetime
{
    private const string s_imageName = "docker.io/library/elasticsearch:9.5.3";

    private readonly ElasticsearchContainer _container = new ElasticsearchBuilder(s_imageName).Build();

    private ElasticsearchClientSettings? _settings;

    /// <summary>
    ///     Gets a client connected to the running Elasticsearch container.
    /// </summary>
    public ElasticsearchClient Client { get; private set; } = null!;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if ( _settings is IDisposable disposable )
        {
            disposable.Dispose();
        }

        await _container.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

        // The container's default configuration enables TLS with a self-signed certificate; trusting it
        // unconditionally is acceptable for this ephemeral, locally-run test container.
        _settings = new ElasticsearchClientSettings(new Uri(_container.GetConnectionString()))
           .ServerCertificateValidationCallback((_, _, _, _) => true);

        Client = new ElasticsearchClient(_settings);
    }
}
