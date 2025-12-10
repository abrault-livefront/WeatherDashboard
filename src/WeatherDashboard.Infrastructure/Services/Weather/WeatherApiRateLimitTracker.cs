namespace WeatherDashboard.Infrastructure.Services.Weather;

using System.Collections.Concurrent;
using Application.Common.Interfaces;

/// <summary>
///     Tracks and enforces rate limits for Weather API requests.
/// </summary>
/// <remarks>
///     This implementation enforces hourly (5,000 requests) and daily (10,000 requests) rate limits
///     using in-memory sliding window tracking. For distributed systems, consider using a shared cache
///     like Redis.
/// </remarks>
internal sealed class WeatherApiRateLimitTracker : IRateLimitTracker, IDisposable
{
    /// <summary>
    ///     Maximum number of API requests allowed per day.
    /// </summary>
    private const int DailyLimit = 10000;

    /// <summary>
    ///     Maximum number of API requests allowed per hour.
    /// </summary>
    private const int HourLimit = 5000;

    /// <summary>
    ///     Maximum number of API requests allowed per minute.
    /// </summary>
    private const int MinuteLimit = 600;

    /// <summary>
    ///     Queue tracking timestamps of requests within the last 24 hours.
    /// </summary>
    private readonly ConcurrentQueue<DateTimeOffset> _dailyRequests = new();

    /// <summary>
    ///     Queue tracking timestamps of requests within the last hour.
    /// </summary>
    private readonly ConcurrentQueue<DateTimeOffset> _hourlyRequests = new();

    /// <summary>
    ///     Queue tracking timestamps of requests within the last minute.
    /// </summary>
    private readonly ConcurrentQueue<DateTimeOffset> _minuteRequests = new();

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly ITimeProvider _timeProvider;

    public WeatherApiRateLimitTracker(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    ///     Determines whether a new API request can be made without exceeding rate limits.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    ///     <see langword="true" /> if the request can be made within minute, hourly and daily limits; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public async Task<bool> CanMakeRequestAsync(CancellationToken cancellationToken = default)
    {
        await CleanupOldRequestsAsync(cancellationToken).ConfigureAwait(false);

        return _minuteRequests.Count < MinuteLimit
            && _hourlyRequests.Count < HourLimit
            && _dailyRequests.Count < DailyLimit;
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    /// <summary>
    ///     Records a new API request timestamp for rate limit tracking.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A completed task.</returns>
    public Task RecordRequestAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset timestamp = _timeProvider.Now;

        _hourlyRequests.Enqueue(timestamp);
        _dailyRequests.Enqueue(timestamp);
        _minuteRequests.Enqueue(timestamp);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Removes expired request timestamps from the tracking queues.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    /// <remarks>
    ///     This method uses a semaphore to ensure thread-safe cleanup of expired entries
    ///     from both hourly and daily request queues.
    /// </remarks>
    private async Task CleanupOldRequestsAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _timeProvider.Now;

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            while ( _minuteRequests.TryPeek(out DateTimeOffset timestamp) && ( now - timestamp ).TotalMinutes >= 1 )
            {
                _minuteRequests.TryDequeue(out _);
            }

            while ( _hourlyRequests.TryPeek(out DateTimeOffset timestamp) && ( now - timestamp ).TotalHours >= 1 )
            {
                _hourlyRequests.TryDequeue(out _);
            }

            while ( _dailyRequests.TryPeek(out DateTimeOffset timestamp) && ( now - timestamp ).TotalDays >= 1 )
            {
                _dailyRequests.TryDequeue(out _);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
