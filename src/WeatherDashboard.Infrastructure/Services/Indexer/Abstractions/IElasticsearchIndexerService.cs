namespace WeatherDashboard.Infrastructure.Services.Indexer.Abstractions;

using Application.Common.Interfaces;
using Domain.Entities.Documents;

/// <summary>
///     Extends <see cref="IIndexerService{TDocument}" /> with Elasticsearch-specific index lifecycle
///     and seed-tracking operations needed by background seeding.
/// </summary>
/// <typeparam name="TDocument">
///     The type of document handled by this indexer. Must implement <see cref="IDocument" />.
/// </typeparam>
public interface IElasticsearchIndexerService<in TDocument> : IIndexerService<TDocument>
    where TDocument : class, IDocument
{
    /// <summary>
    ///     Creates the backing index and its mapping if it does not already exist. Idempotent.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the hash recorded for the last successful seed, or <c>null</c> if the index has never been seeded.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<string?> GetSeedHashAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Drops and recreates the backing index, discarding all documents.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RemoveAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Records the hash of the seed data that was just indexed.
    /// </summary>
    /// <param name="hash">The hash of the seed data that was indexed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveSeedHashAsync(string hash, CancellationToken cancellationToken = default);
}
