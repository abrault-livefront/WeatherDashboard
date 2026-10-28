namespace WeatherDashboard.Infrastructure.Services.Indexer;

using Abstractions;
using Domain.Entities.Documents;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Extensions;
using Microsoft.Extensions.Logging;
using ExistsResponse = Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse;

/// <summary>
///     Provides an implementation of an indexer service using Elasticsearch for indexing location documents.
/// </summary>
public sealed class ElasticsearchLocationIndexerService : IElasticsearchIndexerService<LocationDocument>
{
    private const string s_metadataIndexName = "locations-metadata";

    private const string s_normalizerName = "lowercase_normalizer";

    private const int s_notFoundStatusCode = 404;

    private readonly ElasticsearchClient _client;

    private readonly ILogger<ElasticsearchLocationIndexerService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElasticsearchLocationIndexerService" /> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client used to communicate with the cluster.</param>
    /// <param name="logger">The logger instance used for logging operations within the indexer service.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="client" /> or <paramref name="logger" /> is <c>null</c>.
    /// </exception>
    public ElasticsearchLocationIndexerService(ElasticsearchClient client, ILogger<ElasticsearchLocationIndexerService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string IndexName => "locations";

    /// <inheritdoc />
    public async Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default)
    {
        ExistsResponse existsResponse = await _client.Indices.ExistsAsync(IndexName, cancellationToken).ConfigureAwait(false);
        if ( existsResponse.Exists )
        {
            return;
        }

        CreateIndexResponse response = await _client.Indices.CreateAsync<LocationDocument>(c => c
                                                                                               .Index(IndexName)
                                                                                               .Settings(s => s
                                                                                                             .NumberOfReplicas(0)
                                                                                                             .Analysis(a => a
                                                                                                                 .Normalizers(n => n.Lowercase(s_normalizerName))))
                                                                                               .Mappings(m => m
                                                                                                   .Properties(p => p
                                                                                                                   .Keyword(f => f.Id)
                                                                                                                   .Text(f => f.Locality, t => t.Fields(sub => sub.Keyword("keyword")))
                                                                                                                   .Text(f => f.Province)
                                                                                                                   .Keyword(f => f.ProvinceCode)
                                                                                                                   .Keyword(f => f.PostalCodes, k => k.Normalizer(s_normalizerName))
                                                                                                                   .DoubleNumber(f => f.Latitude)
                                                                                                                   .DoubleNumber(f => f.Longitude))),
                                           cancellationToken).ConfigureAwait(false);

        if ( !response.IsValidResponse )
        {
            _logger.LogFailedToCreateIndex(IndexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to create index '{IndexName}': {response.DebugInformation}");
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetSeedHashAsync(CancellationToken cancellationToken = default)
    {
        GetResponse<SeedMetadata> response =
            await _client.GetAsync<SeedMetadata>(s_metadataIndexName, IndexName, cancellationToken).ConfigureAwait(false);

        return response is { IsValidResponse: true, Found: true, } ? response.Source?.Hash : null;
    }

    /// <inheritdoc />
    public Task IndexAsync(LocationDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return IndexManyAsync([document,], cancellationToken);
    }

    /// <inheritdoc />
    public async Task IndexManyAsync(IEnumerable<LocationDocument> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        BulkResponse response = await _client.BulkAsync(b => b
                                                            .Index(IndexName)
                                                            .IndexMany(documents, (op, doc) => op.Id(doc.Id.ToString())),
                                    cancellationToken).ConfigureAwait(false);

        if ( !response.IsValidResponse || response.Errors )
        {
            _logger.LogFailedToIndexDocuments(IndexName, response.DebugInformation);
            throw new InvalidOperationException($"Bulk indexing into '{IndexName}' failed: {response.DebugInformation}");
        }

        await _client.Indices.RefreshAsync(IndexName, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        DeleteIndexResponse response = await _client.Indices.DeleteAsync(IndexName, cancellationToken).ConfigureAwait(false);

        if ( !response.IsValidResponse && response.ApiCallDetails.HttpStatusCode != s_notFoundStatusCode )
        {
            _logger.LogFailedToDeleteIndex(IndexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to delete index '{IndexName}': {response.DebugInformation}");
        }

        await EnsureIndexExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveSeedHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        await EnsureMetadataIndexExistsAsync(cancellationToken).ConfigureAwait(false);

        await _client.IndexAsync(new SeedMetadata(IndexName, hash, DateTimeOffset.UtcNow),
            s_metadataIndexName,
            IndexName,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureMetadataIndexExistsAsync(CancellationToken cancellationToken)
    {
        ExistsResponse existsResponse = await _client.Indices.ExistsAsync(s_metadataIndexName, cancellationToken).ConfigureAwait(false);
        if ( existsResponse.Exists )
        {
            return;
        }

        CreateIndexResponse response = await _client.Indices.CreateAsync(s_metadataIndexName, c => c
                                              .Settings(s => s.NumberOfReplicas(0)),
                                           cancellationToken).ConfigureAwait(false);

        if ( !response.IsValidResponse )
        {
            _logger.LogFailedToCreateIndex(s_metadataIndexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to create index '{s_metadataIndexName}': {response.DebugInformation}");
        }
    }
}
