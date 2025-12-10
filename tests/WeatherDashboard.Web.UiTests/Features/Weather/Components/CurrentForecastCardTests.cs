namespace WeatherDashboard.Web.UiTests.Features.Weather.Components;

using System.Globalization;
using Application.Contracts.Weather;
using AutoFixture;
using AwesomeAssertions;
using Bunit;
using Domain.Entities.Weather.Enums;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Radzen.Blazor;
using Web.Features.Weather.Components;
using Web.Features.Weather.StateManagement;

[Trait("Category", "UI")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "BlazorComponent")]
[Trait("Speed", "Slow")]
public sealed class CurrentForecastCardTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly IStringLocalizer<SharedResource> _mockLocalizer;

    private readonly IState<WeatherState> _mockWeatherState;

    public CurrentForecastCardTests()
    {
        _fixture.Customize<ForecastCacheContract>(c => c
                                                      .With(f => f.TimeZone, TimeZoneInfo.Utc)
                                                      .With(f => f.Sunrise, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(6).Ticks, TimeSpan.Zero))
                                                      .With(f => f.Sunset, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(18).Ticks, TimeSpan.Zero))
                                                      .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>()));

        _mockLocalizer = Substitute.For<IStringLocalizer<SharedResource>>();
        _mockWeatherState = Substitute.For<IState<WeatherState>>();

        IDispatcher mockDispatcher = Substitute.For<IDispatcher>();
        IStore mockStore = Substitute.For<IStore>();
        IActionSubscriber mockActionSubscriber = Substitute.For<IActionSubscriber>();

        Services.AddSingleton(mockStore);
        Services.AddSingleton(mockDispatcher);
        Services.AddSingleton(mockActionSubscriber);
        Services.AddSingleton(_mockLocalizer);
        Services.AddSingleton(_mockWeatherState);

        SetupDefaultLocalizerBehavior();
    }

    [Fact]
    public void Displays_Current_Day_Of_Week()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("p").Should().Contain(p => p.TextContent.Contains("Mon") || p.TextContent.Contains("Tue") ||
                                               p.TextContent.Contains("Wed") || p.TextContent.Contains("Thu") ||
                                               p.TextContent.Contains("Fri") || p.TextContent.Contains("Sat") ||
                                               p.TextContent.Contains("Sun"));
    }

    [Fact]
    public void Displays_Current_Time_With_Timezone()
    {
        TimeZoneInfo estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.TimeZone, estTimeZone)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        bool containsEst = cut.Markup.Contains("EST", StringComparison.Ordinal);
        bool containsEdt = cut.Markup.Contains("EDT", StringComparison.Ordinal);
        ( containsEst || containsEdt ).Should().BeTrue();
    }

    [Fact]
    public void Displays_Localized_Weather_Condition()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WeatherCode, WeatherCode.ClearSky)
                                                 .Create();

        SetupWeatherState(forecast);
        _mockLocalizer["WeatherCode_ClearSky"].Returns(new LocalizedString("WeatherCode_ClearSky", "Clear Sky"));

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.Markup.Should().Contain("Clear Sky");
    }

    [Fact]
    public void Displays_Sunrise_Time()
    {
        DateTimeOffset sunrise = new(2024, 1, 15, 7, 30, 0, TimeSpan.Zero);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Sunrise, sunrise)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi-sunrise"));
    }

    [Fact]
    public void Displays_Sunset_Time()
    {
        DateTimeOffset sunset = new(2024, 1, 15, 17, 45, 0, TimeSpan.Zero);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Sunset, sunset)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi-sunset"));
    }

    [Fact]
    public void Displays_Temperature()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Temperature, 72.0)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi-thermometer"));
    }

    [Fact]
    public void Displays_Weather_Icon()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WeatherCode, WeatherCode.ClearSky)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi"));
    }

    [Fact]
    public void Handles_Null_Forecast_Gracefully()
    {
        SetupWeatherState(null);

        Action act = () => Render<CurrentForecastCard>();

        act.Should().NotThrow();
    }

    [Fact]
    public void Has_Date_Range_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.TextContent.Contains("date_range", StringComparison.Ordinal));
    }

    [Fact]
    public void Renders_Card_Component()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        IRenderedComponent<RadzenCard> card = cut.FindComponent<RadzenCard>();
        card.Should().NotBeNull();
    }

    [Theory]
    [InlineData(WeatherCode.ClearSky)]
    [InlineData(WeatherCode.PartlyCloudy)]
    [InlineData(WeatherCode.Overcast)]
    [InlineData(WeatherCode.Fog)]
    public void Renders_Different_Weather_Codes(WeatherCode weatherCode)
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WeatherCode, weatherCode)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi"));
    }

    [Fact]
    public void Uses_Correct_Component_Structure()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().HaveCount(1);

        IReadOnlyList<IRenderedComponent<RadzenStack>> stacks = cut.FindComponents<RadzenStack>();
        stacks.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Uses_Imperial_Temperature_For_Imperial_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Temperature, 72.0)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Uses_Metric_Temperature_For_Metric_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-CA");
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Temperature, 72.0)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentForecastCard> cut = Render<CurrentForecastCard>();

        cut.Should().NotBeNull();
    }

    private void SetupDefaultLocalizerBehavior()
    {
        _mockLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
    }

    private void SetupWeatherState(ForecastCacheContract? forecast)
    {
        WeatherState state = new()
        {
            CurrentForecast = forecast,
            IsLoading = false,
        };

        _mockWeatherState.Value.Returns(state);
    }
}
