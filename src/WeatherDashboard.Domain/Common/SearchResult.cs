namespace WeatherDashboard.Domain.Common;

/// <summary>
///     Represents the result of a search operation containing matching documents and the total count.
/// </summary>
/// <typeparam name="T">The type of documents in the search results. Must be a reference type.</typeparam>
/// <param name="Results">The collection of documents matching the search criteria.</param>
/// <param name="TotalCount">The total number of documents matching the search criteria.</param>
public sealed record SearchResult<T>(IReadOnlyList<T> Results, int TotalCount)
    where T : class
{
    /// <summary>
    ///     Gets the collection of documents matching the search criteria.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the value is <see langword="null" />.</exception>
    public IReadOnlyList<T> Results { get; init; } = Results ?? throw new ArgumentNullException(nameof(Results));

    /// <summary>
    ///     Gets the total number of documents matching the search criteria.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public int TotalCount { get; init; } = TotalCount >= 0 ? TotalCount : throw new ArgumentOutOfRangeException(nameof(TotalCount));
}
