namespace WeatherDashboard.Application.Common.Interfaces;

using Domain.Entities.Documents;

/// <summary>
///     Defines a service for indexing documents of type <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document handled by this indexer. Must implement <see cref="IDocument" />.
/// </typeparam>
public interface IIndexerService<in TDocument> : IDisposable
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Gets the directory where the index files are stored.
    /// </summary>
    DirectoryInfo IndexDirectory { get; }

    /// <summary>
    ///     Gets the name of the index.
    /// </summary>
    string IndexName { get; }

    /// <summary>
    ///     Indexes a single document.
    /// </summary>
    /// <param name="document">The document to index.</param>
    /// <exception cref="IOException">Thrown when an I/O error occurs while writing to the index.</exception>
    void Index(TDocument document);

    /// <summary>
    ///     Indexes multiple documents.
    /// </summary>
    /// <param name="documents">The collection of documents to index.</param>
    /// <exception cref="IOException">Thrown when an I/O error occurs while writing to the index.</exception>
    void IndexMany(IEnumerable<TDocument> documents);
}
