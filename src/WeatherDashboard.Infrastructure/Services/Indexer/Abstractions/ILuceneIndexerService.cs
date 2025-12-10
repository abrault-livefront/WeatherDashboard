namespace WeatherDashboard.Infrastructure.Services.Indexer.Abstractions;

using Application.Common.Interfaces;
using Domain.Entities.Documents;
using Lucene.Net.Search;

/// <summary>
///     Defines a contract for Lucene-based indexing services that manage document indexing and searching operations.
/// </summary>
/// <typeparam name="TDocument">The type of document to be indexed. Must implement <see cref="IDocument" />.</typeparam>
public interface ILuceneIndexerService<in TDocument> : IIndexerService<TDocument>
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Gets the <see cref="SearcherManager" /> instance used to manage and provide access to Lucene index searchers.
    /// </summary>
    SearcherManager SearcherManager { get; }

    /// <summary>
    ///     Optimizes the index to improve search performance.
    /// </summary>
    void Optimize();

    /// <summary>
    ///     Removes all documents from the index
    /// </summary>
    void RemoveAll();
}
