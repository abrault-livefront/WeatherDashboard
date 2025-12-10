namespace WeatherDashboard.Infrastructure.Services.Search.Abstractions;

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Documents;
using Extensions;
using Indexer.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides a base implementation for Lucene-based search services that search and retrieve documents of type
///     <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">The type of document to search, which must implement <see cref="IDocument" />.</typeparam>
/// <remarks>
///     This abstract class manages the lifecycle of a Lucene search index, including searcher management,
///     query parsing, and document retrieval. Derived classes must implement the <see cref="FromLuceneDocument" />
///     method to convert Lucene documents back to domain entities.
/// </remarks>
public abstract class LuceneSearchService<TDocument> : ISearchService<TDocument>
    where TDocument : class, IDocument
{
    private readonly ILuceneIndexerService<TDocument> _indexerService;

    private readonly ILogger<LuceneSearchService<TDocument>> _logger;

    private readonly SearcherManager _searcherManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LuceneSearchService{TDocument}" /> class.
    /// </summary>
    /// <param name="indexerService">The indexer service responsible for managing the search index.</param>
    /// <param name="logger">The logger instance for logging search operations and errors.</param>
    /// <param name="luceneVersion">
    ///     The Lucene version to use. Defaults to
    ///     <see cref="Lucene.Net.Util.LuceneVersion.LUCENE_48" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="indexerService" /> or <paramref name="logger" />
    ///     is <c>null</c>.
    /// </exception>
    protected LuceneSearchService(ILuceneIndexerService<TDocument> indexerService,
                                  ILogger<LuceneSearchService<TDocument>> logger,
                                  LuceneVersion luceneVersion = LuceneVersion.LUCENE_48)
    {
        _indexerService = indexerService ?? throw new ArgumentNullException(nameof(indexerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _searcherManager = indexerService.SearcherManager;

        LuceneVersion = luceneVersion;
    }

    /// <summary>
    ///     Gets the Lucene version used for indexing and searching.
    /// </summary>
    protected LuceneVersion LuceneVersion { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public SearchResult<TDocument> Search(string query,
                                          int limit = 10)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        try
        {
            _searcherManager.MaybeRefresh();
            IndexSearcher searcher = _searcherManager.Acquire();

            try
            {
                Query? luceneQuery = CreateQuery(query);
                if ( luceneQuery is null )
                {
                    return new SearchResult<TDocument>([], 0);
                }

                TopDocs hits = searcher.Search(luceneQuery, limit);

                if ( hits.TotalHits <= 0 )
                {
                    return new SearchResult<TDocument>([], hits.TotalHits);
                }

                ReadOnlyCollection<TDocument> results = hits.ScoreDocs
                                                            .Select(s => FromLuceneDocument(searcher.Doc(s.Doc)))
                                                            .Where(w => w is not null)
                                                            .OfType<TDocument>()
                                                            .ToList()
                                                            .AsReadOnly();

                return new SearchResult<TDocument>(results, hits.TotalHits);
            }
            finally
            {
                try
                {
                    _searcherManager.Release(searcher);
                }
                catch ( Exception e )
                {
                    _logger.LogFailedToReleaseSearcher(_indexerService.IndexName, e.Message);
                }
            }
        }
        catch ( ObjectDisposedException )
        {
            throw new ObjectDisposedException(GetType().Name, "The search service has been disposed");
        }
    }

    /// <summary>
    ///     Creates a Lucene query from the specified search query string.
    /// </summary>
    /// <param name="query">The search query string provided by the user.</param>
    /// <returns>
    ///     A <see cref="Query" /> object representing the parsed search query, or <c>null</c> if the query cannot be parsed.
    /// </returns>
    /// <remarks>
    ///     Derived classes must implement this method to define how search queries are constructed,
    ///     including any custom query logic such as fuzzy matching, wildcards, or Boolean operators.
    /// </remarks>
    protected abstract Query? CreateQuery(string query);

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="LuceneSearchService{TDocument}" /> and optionally releases
    ///     the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        // Not implemented by default
    }

    /// <summary>
    ///     Converts a Lucene <see cref="Document" /> to a domain entity of type <typeparamref name="TDocument" />.
    /// </summary>
    /// <param name="document">The Lucene document to convert.</param>
    /// <returns>A domain entity of type <typeparamref name="TDocument" /> populated from the Lucene document.</returns>
    /// <remarks>
    ///     Derived classes must implement this method to define how Lucene documents are mapped back to
    ///     their corresponding domain entities.
    /// </remarks>
    protected abstract TDocument? FromLuceneDocument(Document document);
}
