namespace WeatherDashboard.Infrastructure.UnitTests.Services.Weather;

using Application.Common.Interfaces;
using AwesomeAssertions;
using Infrastructure.Services.Weather;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "RateLimit")]
[Trait("Speed", "Fast")]
public sealed class WeatherApiRateLimitTrackerTests
{
    [Fact]
    public async Task CanMakeRequestAsync_ConcurrentRequests_ThreadSafe()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.Now);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        const int concurrentRequests = 100;
        Task<bool>[] tasks = new Task<bool>[concurrentRequests];

        for ( int i = 0; i < concurrentRequests; i++ )
        {
            tasks[i] = tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);
        }

        bool[] results = await Task.WhenAll(tasks);

        foreach ( bool result in results )
        {
            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanMakeRequestAsync_WhenExceedingDailyLimit_ReturnsFalse()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.Now);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        // Simulate exceeding the daily limit
        for ( int i = 0; i < 10001; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeFalse();
    }

    [Fact]
    public async Task CanMakeRequestAsync_WhenExceedingHourLimit_ReturnsFalse()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.Now);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        // Simulate exceeding the hour limit
        for ( int i = 0; i < 5001; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeFalse();
    }

    [Fact]
    public async Task CanMakeRequestAsync_WhenExceedingMinuteLimit_ReturnsFalse()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.Now);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        // Simulate exceeding the minute limit
        for ( int i = 0; i < 601; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeFalse();
    }

    [Fact]
    public async Task CanMakeRequestAsync_WhenUnderAllLimits_ReturnsTrue()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.Now);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupOldRequestsAsync_RemovesExpiredDailyRequests()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        for ( int i = 0; i < 10000; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        timeProvider.Advance(TimeSpan.FromHours(25));
        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupOldRequestsAsync_RemovesExpiredHourlyRequests()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        for ( int i = 0; i < 5000; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        timeProvider.Advance(TimeSpan.FromMinutes(61));
        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupOldRequestsAsync_RemovesExpiredMinuteRequests()
    {
        FakeTimeProvider timeProvider = new(DateTimeOffset.UtcNow);
        using WeatherApiRateLimitTracker tracker = new(timeProvider);

        for ( int i = 0; i < 600; i++ )
        {
            await tracker.RecordRequestAsync(TestContext.Current.CancellationToken);
        }

        timeProvider.Advance(TimeSpan.FromMinutes(2));
        bool canMakeRequest = await tracker.CanMakeRequestAsync(TestContext.Current.CancellationToken);

        canMakeRequest.Should().BeTrue();
    }

    private class FakeTimeProvider : ITimeProvider
    {
        public FakeTimeProvider(DateTimeOffset startTime)
        {
            Now = startTime;
        }

        public DateTimeOffset Now { get; private set; }

        public void Advance(TimeSpan duration)
        {
            Now = Now.Add(duration);
        }
    }
}
