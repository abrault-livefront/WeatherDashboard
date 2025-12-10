namespace WeatherDashboard.Infrastructure.UnitTests.Services.Weather;

using System.Globalization;
using System.Net;
using System.Text.Json;
using Application.Common.Interfaces;
using AutoFixture;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Entities.Weather;
using Infrastructure.Services.Weather;
using Infrastructure.Services.Weather.Responses;
using NSubstitute;
using RichardSzalay.MockHttp;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Weather")]
[Trait("Speed", "Fast")]
public sealed class WeatherServiceTests
{
    private const string ApiUrl = "https://api.open-meteo.com/v1/forecast";

    private readonly Fixture _fixture = new();

    public WeatherServiceTests()
    {
        _fixture.Customize<DateOnly>(c =>
        {
            return c.FromFactory(() => DateOnly.FromDateTime(DateTime.Now));
        });
    }

    [Theory]
    [AutoData]
    public async Task RequestAsync_WhenClientReturnsNotFound_ShouldReturnNull(double latitude, double longitude)
    {
        using MockHttpMessageHandler mockHttp = new();

        mockHttp.When(HttpMethod.Get, ApiUrl)
                .Respond(HttpStatusCode.NotFound);

        IRateLimitTracker? rateLimitTracker = Substitute.For<IRateLimitTracker>();
        rateLimitTracker.CanMakeRequestAsync(Arg.Any<CancellationToken>()).Returns(true);

        WeatherApiClient apiClient = new(mockHttp.ToHttpClient(), rateLimitTracker);
        WeatherService service = new(apiClient);

        Forecast? result = await service.RequestAsync(latitude, longitude, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public async Task RequestAsync_WhenRateLimitExceeded_ShouldThrowInvalidOperationException(double latitude, double longitude)
    {
        using MockHttpMessageHandler mockHttp = new();

        mockHttp.When(HttpMethod.Get, ApiUrl)
                .Respond(HttpStatusCode.OK);

        IRateLimitTracker? rateLimitTracker = Substitute.For<IRateLimitTracker>();
        rateLimitTracker.CanMakeRequestAsync(Arg.Any<CancellationToken>()).Returns(false);

        WeatherApiClient apiClient = new(mockHttp.ToHttpClient(), rateLimitTracker);
        WeatherService service = new(apiClient);

        Func<Task> act = () => service.RequestAsync(latitude, longitude, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoData]
    public async Task RequestAsync_WithCancellationRequested_ShouldReturnNull(double latitude, double longitude)
    {
        using MockHttpMessageHandler mockHttp = new();

        IRateLimitTracker? rateLimitTracker = Substitute.For<IRateLimitTracker>();
        rateLimitTracker.CanMakeRequestAsync(Arg.Any<CancellationToken>()).Returns(true);

        WeatherApiClient apiClient = new(mockHttp.ToHttpClient(), rateLimitTracker);
        WeatherService service = new(apiClient);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Forecast? result = await service.RequestAsync(latitude, longitude, cts.Token);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public async Task RequestAsync_WithSpecificCoordinates_ShouldPassCorrectParametersToClient(double latitude, double longitude)
    {
        using MockHttpMessageHandler mockHttp = new();

        WeatherApiResponse? apiResponse = _fixture.Build<WeatherApiResponse>()
                                                  .With(w => w.Latitude, latitude)
                                                  .With(w => w.Longitude, longitude)
                                                  .Create();

        mockHttp.When(HttpMethod.Get, ApiUrl)
                .WithQueryString(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                 {
                     { "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                     { "longitude", longitude.ToString(CultureInfo.InvariantCulture) },
                     { "daily", "apparent_temperature_max,dew_point_2m_max,relative_humidity_2m_min,sunrise,sunset,surface_pressure_min,uv_index_max,visibility_mean,weather_code,wind_direction_10m_dominant,wind_gusts_10m_min,wind_speed_10m_min" },
                     { "wind_speed_unit", "mph" },
                     { "temperature_unit", "fahrenheit" },
                     { "precipitation_unit", "inch" },
                     { "timezone", "auto" },
                 })
                .Respond("application/json", JsonSerializer.Serialize(apiResponse));

        IRateLimitTracker? rateLimitTracker = Substitute.For<IRateLimitTracker>();
        rateLimitTracker.CanMakeRequestAsync(Arg.Any<CancellationToken>()).Returns(true);

        WeatherApiClient apiClient = new(mockHttp.ToHttpClient(), rateLimitTracker);
        WeatherService service = new(apiClient);

        Forecast? result = await service.RequestAsync(latitude, longitude, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Latitude.Should().Be(latitude);
        result.Longitude.Should().Be(longitude);
    }
}
