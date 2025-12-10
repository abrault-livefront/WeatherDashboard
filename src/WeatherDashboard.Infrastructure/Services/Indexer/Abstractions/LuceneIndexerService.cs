namespace WeatherDashboard.Infrastructure.Services.Indexer.Abstractions;

using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Documents;
using Extensions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Persistence;

/// <summary>
///     Provides an abstract implementation of an indexer service using Apache Lucene.NET for indexing and managing
///     documents of type <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document to be indexed. Must be a reference type that implements <see cref="IDocument" />.
/// </typeparam>
/// <remarks>
///     This abstract class manages the lifecycle of a Lucene index writer, including near-real-time (NRT) search support,
///     transaction management, and thread-safe indexing operations. Derived classes must implement the
///     <see cref="ToLuceneDocument" />
///     method to convert domain entities to Lucene documents.
/// </remarks>
public abstract class LuceneIndexerService<TDocument> : ILuceneIndexerService<TDocument>
    where TDocument : class, IDocument
{
    private const long JoinTimeoutMilliseconds = 5000;

    private readonly StandardAnalyzer _analyzer;

    private readonly ILogger<LuceneIndexerService<TDocument>> _logger;

    private readonly ControlledRealTimeReopenThread<IndexSearcher> _nrtThread;

    private readonly TrackingIndexWriter _trackingWriter;

    private readonly IndexWriter _writer;

    private int _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LuceneIndexerService{TDocument}" /> class.
    /// </summary>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="indexDirectory">The directory where the index files are stored.</param>
    /// <param name="logger">The logger instance for logging indexer operations and errors.</param>
    /// <param name="directoryFactory">The factory for creating and managing shared Lucene directory instances.</param>
    /// <param name="luceneVersion">The Lucene version to use. Defaults to <see cref="LuceneVersion.LUCENE_48" />.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="indexName" /> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="indexDirectory" />, <paramref name="logger" />, or <paramref name="directoryFactory" />
    ///     is <c>null</c>.
    /// </exception>
    protected LuceneIndexerService(string indexName,
                                   DirectoryInfo indexDirectory,
                                   ILogger<LuceneIndexerService<TDocument>> logger,
                                   LuceneDirectoryFactory directoryFactory,
                                   LuceneVersion luceneVersion = LuceneVersion.LUCENE_48)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentNullException.ThrowIfNull(indexDirectory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(directoryFactory);

        IndexName = indexName;
        IndexDirectory = new DirectoryInfo(Path.Combine(indexDirectory.FullName, indexName));

        MMapDirectory directory = directoryFactory.GetOrCreateDirectory(IndexDirectory);

        _analyzer = new StandardAnalyzer(luceneVersion);
        _logger = logger;
        _writer = new IndexWriter(directory, new IndexWriterConfig(luceneVersion, _analyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND,
        });

        _trackingWriter = new TrackingIndexWriter(_writer);

        SearcherManager = new SearcherManager(_writer, true, new SearcherFactory());

        _nrtThread = new ControlledRealTimeReopenThread<IndexSearcher>(
            _trackingWriter, SearcherManager, 1.0, 0.25)
        {
            Name = $"NRT Reopen Thread for {IndexName} Indexer",
        };

        _nrtThread.Start();
    }

    /// <inheritdoc />
    public DirectoryInfo IndexDirectory { get; }

    /// <inheritdoc />
    public string IndexName { get; }

    /// <inheritdoc />
    public SearcherManager SearcherManager { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Index(TDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        IndexMany([document,]);
    }

    /// <inheritdoc />
    public void IndexMany(IEnumerable<TDocument> documents)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
        ArgumentNullException.ThrowIfNull(documents);

        long generation = 0;

        try
        {
            foreach ( TDocument document in documents )
            {
                Document doc = ToLuceneDocument(document);

                Term identifier = new(nameof(document.Id), document.Id.ToString());

                generation = _trackingWriter.UpdateDocument(identifier, doc);
            }

            WaitForGeneration(generation);

            _writer.Commit();

            SearcherManager.MaybeRefreshBlocking();
        }
        catch ( CorruptIndexException )
        {
            _logger.LogCorruptLuceneIndex(IndexName);

            try
            {
                _writer.Rollback();
            }
            catch ( IOException e )
            {
                _logger.LogFailedToRollback(IndexName, e.Message);
            }

            throw;
        }
        catch ( Exception ex )
        {
            _logger.LogFailedToIndexDocuments(IndexName, ex.Message);

            try
            {
                _writer.Rollback();
            }
            catch ( IOException e )
            {
                _logger.LogFailedToRollback(IndexName, e.Message);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public void Optimize()
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);

        _writer.Commit();
        _writer.ForceMerge(1);
        _writer.Commit();

        SearcherManager.MaybeRefreshBlocking();
    }

    /// <inheritdoc />
    public void RemoveAll()
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);

        _writer.DeleteAll();
        _writer.ForceMerge(1);
        _writer.Commit();

        SearcherManager.MaybeRefreshBlocking();
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="LuceneIndexerService{TDocument}" /> and optionally releases
    ///     the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    ///     This method stops the NRT reopen thread, commits any pending changes to the index, and disposes of all Lucene.NET
    ///     resources.
    ///     If the commit fails during disposal, the exception is logged but not propagated.
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    protected virtual void Dispose(bool disposing)
    {
        if ( Interlocked.CompareExchange(ref _disposed, 1, 0) != 0 )
        {
            return;
        }

        if ( !disposing )
        {
            return;
        }

        _nrtThread.Dispose();
        _nrtThread.Join(JoinTimeoutMilliseconds);

        try
        {
            _writer.Commit();
        }
        catch ( Exception e )
        {
            _logger.LogFailedToCommitOnDispose(IndexName, e.Message);
        }

        SearcherManager.Dispose();

        _writer.Dispose();
        _analyzer.Dispose();
    }

    /// <summary>
    ///     Converts a <typeparamref name="TDocument" /> instance into a Lucene.NET <see cref="Document" /> for indexing.
    /// </summary>
    /// <param name="document">The document to convert.</param>
    /// <returns>A Lucene.NET <see cref="Document" /> containing the indexed fields from the source document.</returns>
    protected abstract Document ToLuceneDocument(TDocument document);

    private void WaitForGeneration(long generation)
    {
        const int pollingIntervalMilliseconds = 50;

        while ( !_nrtThread.WaitForGeneration(generation, pollingIntervalMilliseconds) )
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
        }
    }
}
