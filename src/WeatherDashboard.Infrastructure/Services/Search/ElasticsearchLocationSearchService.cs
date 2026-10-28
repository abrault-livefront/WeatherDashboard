namespace WeatherDashboard.Infrastructure.Services.Search;

using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Documents;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Extensions;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides search functionality for location documents using Elasticsearch full-text search.
/// </summary>
/// <remarks>
///     This service searches across locality names and postal codes. Locality matching uses a
///     "search as you type" style query so partially-typed terms still match, while postal code
///     matching uses a case-insensitive prefix match.
/// </remarks>
public sealed class ElasticsearchLocationSearchService : ISearchService<LocationDocument>
{
    private const string s_indexName = "locations";

    private readonly ElasticsearchClient _client;

    private readonly ILogger<ElasticsearchLocationSearchService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElasticsearchLocationSearchService" /> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client used to communicate with the cluster.</param>
    /// <param name="logger">The logger instance for recording search operations and errors.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="client" /> or <paramref name="logger" /> is <c>null</c>.
    /// </exception>
    public ElasticsearchLocationSearchService(ElasticsearchClient client, ILogger<ElasticsearchLocationSearchService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SearchResult<LocationDocument>> SearchAsync(string query,
                                                                   int limit = 10,
                                                                   CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        if ( !LocationSearchQuerySanitizer.TrySanitize(query, out string sanitized) )
        {
            return new SearchResult<LocationDocument>([], 0);
        }

        SearchResponse<LocationDocument> response = await _client.SearchAsync<LocationDocument>(s => s
                .Indices(s_indexName)
                .Size(limit)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.MatchBoolPrefix(mbp => mbp
                                .Field(f => f.Locality)
                                .Query(sanitized)
                                .Operator(Operator.And)),
                            sh => sh.Prefix(pr => pr
                                .Field(f => f.PostalCodes)
                                .Value(sanitized)))
                        .MinimumShouldMatch(1))),
            cancellationToken).ConfigureAwait(false);

        if ( !response.IsValidResponse )
        {
            _logger.LogFailedToSearch(s_indexName, response.DebugInformation);
            throw new InvalidOperationException($"Elasticsearch search against '{s_indexName}' failed: {response.DebugInformation}");
        }

        return new SearchResult<LocationDocument>(response.Documents.ToList(), (int)response.Total);
    }
}
