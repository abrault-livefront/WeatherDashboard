namespace WeatherDashboard.Application.Common.Interfaces;

using Domain.Entities.Documents;

/// <summary>
///     Defines a service for indexing documents of type <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document handled by this indexer. Must implement <see cref="IDocument" />.
/// </typeparam>
public interface IIndexerService<in TDocument>
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Gets the name of the index.
    /// </summary>
    string IndexName { get; }

    /// <summary>
    ///     Indexes a single document.
    /// </summary>
    /// <param name="document">The document to index.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task IndexAsync(TDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Indexes multiple documents.
    /// </summary>
    /// <param name="documents">The collection of documents to index.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task IndexManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);
}
