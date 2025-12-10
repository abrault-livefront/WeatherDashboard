namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Effects;

using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Contracts.Weather;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Entities.Weather;
using Domain.Entities.Weather.Enums;
using Fluxor;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Effects;
using ZiggyCreatures.Caching.Fusion;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "Effect")]
[Trait("Speed", "Fast")]
public sealed class FetchWeatherEffectTests : IDisposable
{
    private readonly FusionCache _cache;

    private readonly IDispatcher _mockDispatcher;

    private readonly IWeatherService _mockWeatherService;

    public FetchWeatherEffectTests()
    {
        // Create a real FusionCache with MemoryCache backend for testing
        FusionCacheOptions options = new();
        #pragma warning disable CA2000 // Disposed by FusionCache
        _cache = new FusionCache(options, new MemoryCache(new MemoryCacheOptions()));
        #pragma warning restore CA2000

        _mockWeatherService = Substitute.For<IWeatherService>();
        _mockDispatcher = Substitute.For<IDispatcher>();
    }

    [Fact]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        IFusionCache? cache = null;

        #pragma warning disable CA1806
        Action act = () => _ = new FetchWeatherEffect(cache!, _mockWeatherService);
        #pragma warning restore CA1806

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("cache");
    }

    [Fact]
    public void Constructor_WithNullWeatherService_ThrowsArgumentNullException()
    {
        IWeatherService? weatherService = null;

        #pragma warning disable CA1806
        Action act = () => _ = new FetchWeatherEffect(_cache, weatherService!);
        #pragma warning restore CA1806

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("weatherService");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesEffect()
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);

        effect.Should().NotBeNull();
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_UsesCorrectCacheKey(double latitude, double longitude)
    {
        string expectedCacheKey = HashUtility.HashString($"weather-{latitude}-{longitude}");

        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        Forecast forecast = CreateForecast(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns(forecast);

        await effect.HandleAsync(action, _mockDispatcher);

        // Verify the cache contains the expected key
        ForecastCacheContract cachedValue = await _cache.TryGetAsync<ForecastCacheContract>(expectedCacheKey,
                                                token: TestContext.Current.CancellationToken);

        cachedValue.Should().NotBeNull();
        cachedValue.Latitude.Should().Be(latitude);
        cachedValue.Longitude.Should().Be(longitude);
    }

    [Fact]
    public async Task HandleAsync_WithBothInfiniteCoordinates_DispatchesFailureAction()
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(double.PositiveInfinity, double.NegativeInfinity);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherFailureAction>());
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithCacheHit_DoesNotCallWeatherService(double latitude, double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        Forecast forecast = CreateForecast(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns(forecast);

        // First call - cache miss
        await effect.HandleAsync(action, _mockDispatcher);

        // Second call - cache hit
        await effect.HandleAsync(action, _mockDispatcher);

        // Weather service should only be called once (first call)
        await _mockWeatherService.Received(1).RequestAsync(latitude, longitude, Arg.Any<CancellationToken>());
        // Dispatcher should be called twice (both calls)
        _mockDispatcher.Received(2).Dispatch(Arg.Any<FetchWeatherSuccessAction>());
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithCacheMiss_CallsWeatherService(double latitude, double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        Forecast forecast = CreateForecast(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns(forecast);

        await effect.HandleAsync(action, _mockDispatcher);

        await _mockWeatherService.Received(1).RequestAsync(latitude, longitude, Arg.Any<CancellationToken>());
        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherSuccessAction>());
    }

    [Fact]
    public async Task HandleAsync_WithInfiniteLatitude_DispatchesFailureAction()
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(double.PositiveInfinity, -74.0060);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherFailureAction>());
    }

    [Fact]
    public async Task HandleAsync_WithInfiniteLongitude_DispatchesFailureAction()
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(40.7128, double.NegativeInfinity);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherFailureAction>());
    }

    [Fact]
    public async Task HandleAsync_WithNullAction_ThrowsArgumentNullException()
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction? action = null;

        Func<Task> act = async () => await effect.HandleAsync(action!, _mockDispatcher).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("action")
                 .ConfigureAwait(true);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithNullDispatcher_ThrowsArgumentNullException(double latitude, double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        IDispatcher? dispatcher = null;

        Func<Task> act = async () => await effect.HandleAsync(action, dispatcher!).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("dispatcher")
                 .ConfigureAwait(true);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidCoordinates_DispatchesSuccessAction(double latitude, double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        Forecast forecast = CreateForecast(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns(forecast);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<FetchWeatherSuccessAction>(a => a.Forecast.Latitude.Equals(latitude)
                                                                                 && a.Forecast.Longitude.Equals(longitude)));
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(90.0, 180.0)]
    [InlineData(-90.0, -180.0)]
    [InlineData(51.5074, -0.1278)]
    [InlineData(35.6762, 139.6503)]
    public async Task HandleAsync_WithVariousValidCoordinates_DispatchesSuccessAction(
        double latitude,
        double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);
        Forecast forecast = CreateForecast(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns(forecast);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<FetchWeatherSuccessAction>(a => a.Forecast.Latitude.Equals(latitude) &&
                                                                                    a.Forecast.Longitude.Equals(longitude)));
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithWeatherServiceReturningNull_DispatchesFailureAction(double latitude, double longitude)
    {
        FetchWeatherEffect effect = new(_cache, _mockWeatherService);
        FetchWeatherAction action = new(latitude, longitude);

        _mockWeatherService.RequestAsync(latitude, longitude, Arg.Any<CancellationToken>())
                           .Returns((Forecast?)null);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherFailureAction>());
    }

    private static Forecast CreateForecast(double latitude, double longitude)
    {
        return new Forecast
        {
            Latitude = latitude,
            Longitude = longitude,
            Temperature = 72.5,
            DewPoint = 55.3,
            Humidity = 60,
            SurfacePressure = 29.92,
            WindSpeed = 10.5,
            WindGusts = 15.2,
            WindDirection = 180,
            Visibility = 10.0,
            UvIndex = 5.0,
            WeatherCode = WeatherCode.ClearSky,
            Sunrise = DateTimeOffset.UtcNow.AddHours(-2),
            Sunset = DateTimeOffset.UtcNow.AddHours(6),
            TimeZone = TimeZoneInfo.Utc,
            FutureForecasts = new Dictionary<DateOnly, Forecast>(),
        };
    }
}
