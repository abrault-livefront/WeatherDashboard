namespace WeatherDashboard.Infrastructure.UnitTests.Services.Weather;

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Entities.Weather;
using Domain.Entities.Weather.Enums;
using Infrastructure.Services.Weather;
using Infrastructure.Services.Weather.Responses;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Weather")]
[Trait("Speed", "Fast")]
public sealed class WeatherMapperTests
{
    private readonly Fixture _fixture = new();

    public WeatherMapperTests()
    {
        _fixture.Customize<DateOnly>(c =>
        {
            return c.FromFactory(() => DateOnly.FromDateTime(DateTime.Now));
        });

        _fixture.Customize<TimeZoneInfo>(c =>
        {
            return c.FromFactory(() => TimeZoneInfo.Utc);
        });
    }

    [Theory]
    [AutoData]
    public void Map_FutureForecastsKeyMapping_MatchesSourceTimes(double latitude, double longitude)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(latitude, longitude, timeZone, itemCount: 5);

        Forecast result = WeatherMapper.Map(source);

        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, timeZone));

        result.FutureForecasts.Keys.Should()
              .BeSubsetOf(source.Items.Times)
              .And.NotContain(today);
    }

    [Theory]
    [AutoData]
    public void Map_TemperatureAndDewPoint_AreRoundedAwayFromZero(double latitude, double longitude)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(latitude, longitude, timeZone, 20.5, 10.5);

        Forecast result = WeatherMapper.Map(source);

        result.DewPoint.Should().Be(11);
        result.Temperature.Should().Be(21);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => WeatherMapper.Map(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("source");
    }

    [Theory]
    [AutoData]
    public void Map_WithSingleDayData_ReturnsForecastWithNoFutureForecasts(double latitude, double longitude)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(latitude, longitude, timeZone, itemCount: 1);

        Forecast result = WeatherMapper.Map(source);

        result.FutureForecasts.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void Map_WithValidSource_ExcludesTodayFromFutureForecasts(double latitude, double longitude, double temperature, double dewPoint)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(
            latitude,
            longitude,
            timeZone,
            temperature,
            dewPoint,
            3);

        Forecast result = WeatherMapper.Map(source);

        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, timeZone));

        result.FutureForecasts.Should().NotContainKey(today);
    }

    [Theory]
    [AutoData]
    public void Map_WithValidSource_MapsAllWeatherAttributes(double latitude, double longitude)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(latitude, longitude, timeZone);

        Forecast result = WeatherMapper.Map(source);

        result.Should().NotBeNull();
        result.Temperature.Should().BeGreaterThanOrEqualTo(-100).And.BeLessThanOrEqualTo(100);
        result.DewPoint.Should().BeGreaterThanOrEqualTo(-100).And.BeLessThanOrEqualTo(100);
        result.Humidity.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(100);
        result.SurfacePressure.Should().BeGreaterThan(0);
        result.Visibility.Should().BeGreaterThanOrEqualTo(0);
        result.WindSpeed.Should().BeGreaterThanOrEqualTo(0);
        result.WindGusts.Should().BeGreaterThanOrEqualTo(0);
        result.WindDirection.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(360);
        result.UvIndex.Should().BeGreaterThanOrEqualTo(0);
        result.WeatherCode.Should().BeDefined();
        result.Sunrise.Should().NotBe(default);
        result.Sunset.Should().NotBe(default);
    }

    [Theory]
    [AutoData]
    public void Map_WithValidSource_PopulatesFutureForecastsCorrectly(double latitude, double longitude, int itemCount)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(
            latitude,
            longitude,
            timeZone,
            itemCount: itemCount);

        Forecast result = WeatherMapper.Map(source);

        result.FutureForecasts.Should().HaveCount(itemCount - 1);

        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, timeZone));

        result.FutureForecasts.Keys.Should()
              .BeSubsetOf(source.Items.Times)
              .And.NotContain(today);
    }

    [Theory]
    [AutoData]
    public void Map_WithValidSource_PopulatesTodayForecastCorrectly(double latitude, double longitude, double temperature, double dewPoint)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(
            latitude,
            longitude,
            timeZone,
            temperature,
            dewPoint,
            3);

        Forecast result = WeatherMapper.Map(source);

        result.Temperature.Should().Be(Math.Round(temperature, 0, MidpointRounding.AwayFromZero));
        result.DewPoint.Should().Be(Math.Round(dewPoint, 0, MidpointRounding.AwayFromZero));
    }

    [Theory]
    [AutoData]
    public void Map_WithValidSource_ReturnsForecastWithCorrectLocationData(double latitude, double longitude)
    {
        TimeZoneInfo timeZone = _fixture.Create<TimeZoneInfo>();
        WeatherApiResponse source = CreateWeatherApiResponse(latitude, longitude, timeZone);

        Forecast result = WeatherMapper.Map(source);

        result.Latitude.Should().Be(latitude);
        result.Longitude.Should().Be(longitude);
        result.TimeZone.Should().Be(timeZone);
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Fixture Generator")]
    private WeatherApiResponse CreateWeatherApiResponse(
        double latitude,
        double longitude,
        TimeZoneInfo timeZone,
        double? temperature = null,
        double? dewPoint = null,
        int itemCount = 5)
    {
        DateOnly[] times = new DateOnly[itemCount];
        DateOnly baseDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, timeZone));

        for ( int i = 0; i < itemCount; i++ )
        {
            times[i] = baseDate.AddDays(i);
        }

        double[] temps = new double[itemCount];
        double[] dewPoints = new double[itemCount];

        for ( int i = 0; i < itemCount; i++ )
        {
            temps[i] = temperature ?? ( _fixture.Create<double>() % 40 ) - 20;
            dewPoints[i] = dewPoint ?? ( _fixture.Create<double>() % 20 ) - 10;
        }

        return new WeatherApiResponse
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = timeZone,
            Items = new ForecastResponse
            {
                Times = times,
                Temperatures = temps,
                DewPoints = dewPoints,
                Humidity =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<int>() % 101),
                ],
                SurfacePressures =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => ( _fixture.Create<double>() % 100 ) + 900),
                ],
                Visibilities =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<double>() % 10000),
                ],
                WindSpeeds =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<double>() % 50),
                ],
                WindGusts =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<double>() % 80),
                ],
                WindDirections =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<int>() % 361),
                ],
                UvIndexes =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<int>() % 12),
                ],
                WeatherCodes =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => _fixture.Create<WeatherCode>()),
                ],
                SunriseTimes =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => DateTime.UtcNow.Date),
                ],
                SunsetTimes =
                [
                    .. Enumerable.Range(1, itemCount)
                                 .Select(_ => DateTime.UtcNow.Date.AddHours(18)),
                ],
            },
        };
    }
}
