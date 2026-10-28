namespace WeatherDashboard.Application.Common.Interfaces;

using Domain.Common;
using Domain.Entities.Documents;

/// <summary>
///     Defines a service for searching indexed documents of type <typeparamref name="TDocument" />.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document handled by this search service. Must implement <see cref="IDocument" />.
/// </typeparam>
public interface ISearchService<TDocument>
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Searches the index for documents matching the specified query.
    /// </summary>
    /// <param name="query">The search query string to execute against the index.</param>
    /// <param name="limit">The maximum number of results to return. Must be greater than zero. Defaults to 10.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A <see cref="SearchResult{TDocument}" /> with the matching documents and total hit count.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="query" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="limit" /> is less than or equal to zero.
    /// </exception>
    Task<SearchResult<TDocument>> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
}
