namespace WeatherDashboard.Application.Common.Interfaces;

using Domain.Common;
using Domain.Entities.Documents;

/// <summary>
///     Defines a service for searching indexed documents of type <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document handled by this search service. Must implement <see cref="IDocument" />.
/// </typeparam>
public interface ISearchService<TDocument> : IDisposable
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Searches the Lucene index for documents matching the specified query across the given fields.
    /// </summary>
    /// <param name="query">The search query string to execute against the index.</param>
    /// <param name="limit">The maximum number of results to return. Must be greater than zero. Defaults to 10.</param>
    /// <returns>
    ///     A <see cref="SearchResult{TDocument}" /> with the matching documents and total hit count.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when the search service has been disposed.</exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="query" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="limit" /> is less than or equal to zero.
    /// </exception>
    /// <remarks>
    ///     This method refreshes the searcher if necessary, parses the query using a multi-field parser,
    ///     and returns an empty result set if the query cannot be parsed or contains too many clauses.
    /// </remarks>
    SearchResult<TDocument> Search(string query, int limit = 10);
}
