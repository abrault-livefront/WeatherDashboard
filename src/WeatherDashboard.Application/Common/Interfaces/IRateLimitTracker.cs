namespace WeatherDashboard.Application.Common.Interfaces;

/// <summary>
///     Defines a contract for tracking and enforcing rate limits on API requests.
/// </summary>
public interface IRateLimitTracker
{
    /// <summary>
    ///     Determines whether a request can be made based on current rate limit constraints.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a request can be
    ///     made; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> CanMakeRequestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Records that a request has been made for rate limit tracking purposes.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RecordRequestAsync(CancellationToken cancellationToken = default);
}
