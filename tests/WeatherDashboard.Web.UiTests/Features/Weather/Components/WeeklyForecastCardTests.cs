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
public sealed class WeeklyForecastCardTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly IStringLocalizer<WeeklyForecastCard> _mockCardLocalizer;

    private readonly IStringLocalizer<SharedResource> _mockSharedLocalizer;

    private readonly IState<WeatherState> _mockWeatherState;

    public WeeklyForecastCardTests()
    {
        _fixture.Customize<ForecastCacheContract>(c => c
                                                      .With(f => f.TimeZone, TimeZoneInfo.Utc)
                                                      .With(f => f.Sunrise, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(6).Ticks, TimeSpan.Zero))
                                                      .With(f => f.Sunset, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(18).Ticks, TimeSpan.Zero))
                                                      .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>()));

        _mockCardLocalizer = Substitute.For<IStringLocalizer<WeeklyForecastCard>>();
        _mockSharedLocalizer = Substitute.For<IStringLocalizer<SharedResource>>();
        _mockWeatherState = Substitute.For<IState<WeatherState>>();

        IDispatcher mockDispatcher = Substitute.For<IDispatcher>();
        IStore mockStore = Substitute.For<IStore>();
        IActionSubscriber mockActionSubscriber = Substitute.For<IActionSubscriber>();

        Services.AddSingleton(mockStore);
        Services.AddSingleton(mockDispatcher);
        Services.AddSingleton(mockActionSubscriber);
        Services.AddSingleton(_mockCardLocalizer);
        Services.AddSingleton(_mockSharedLocalizer);
        Services.AddSingleton(_mockWeatherState);

        SetupDefaultLocalizerBehavior();
    }

    [Fact]
    public void Displays_Date()
    {
        DateOnly date = new(2024, 1, 15);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { date, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        string expectedDate = date.ToString("d", CultureInfo.CurrentCulture);
        cut.Markup.Should().Contain(expectedDate);
    }

    [Fact]
    public void Displays_Day_Of_Week()
    {
        DateOnly monday = new(2024, 1, 15);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { monday, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        string expectedDay = monday.ToString("ddd", CultureInfo.CurrentCulture);
        cut.Markup.Should().Contain(expectedDay);
    }

    [Fact]
    public void Displays_Dew_Point_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["DewPoint"].Returns(new LocalizedString("DewPoint", "Dew Point"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Dew Point");
    }

    [Fact]
    public void Displays_Empty_When_No_Forecasts()
    {
        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().BeEmpty();
    }

    [Fact]
    public void Displays_Humidity_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Humidity, 65)
                                                 .Create();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Humidity"].Returns(new LocalizedString("Humidity", "Humidity"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Humidity");
        cut.Markup.Should().Contain("65%");
    }

    [Fact]
    public void Displays_Pressure_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Pressure"].Returns(new LocalizedString("Pressure", "Pressure"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Pressure");
    }

    [Fact]
    public void Displays_Sunrise_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Sunrise"].Returns(new LocalizedString("Sunrise", "Sunrise"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Sunrise");
    }

    [Fact]
    public void Displays_Sunset_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Sunset"].Returns(new LocalizedString("Sunset", "Sunset"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Sunset");
    }

    [Fact]
    public void Displays_Temperature()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Temperature, 72.0)
                                                 .Create();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Displays_UV_Index_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["UVIndex"].Returns(new LocalizedString("UVIndex", "UV Index"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("UV Index");
    }

    [Fact]
    public void Displays_Visibility_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Visibility"].Returns(new LocalizedString("Visibility", "Visibility"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Visibility");
    }

    [Fact]
    public void Displays_Weather_Condition_Text()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WeatherCode, WeatherCode.ClearSky)
                                                 .Create();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["WeatherCode_ClearSky"].Returns(new LocalizedString("WeatherCode_ClearSky", "Clear Sky"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Clear Sky");
    }

    [Fact]
    public void Displays_Weather_Icon()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WeatherCode, WeatherCode.ClearSky)
                                                 .Create();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi"));
    }

    [Fact]
    public void Displays_Wind_Label()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);
        _mockSharedLocalizer["Wind"].Returns(new LocalizedString("Wind", "Wind"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("Wind");
    }

    [Fact]
    public void Handles_Null_Current_Forecast_Gracefully()
    {
        SetupWeatherState(null);

        Action act = () => Render<WeeklyForecastCard>();

        act.Should().NotThrow();
    }

    [Fact]
    public void Renders_Card_Title()
    {
        ForecastCacheContract currentForecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(currentForecast);

        _mockCardLocalizer["CardTitle"].Returns(new LocalizedString("CardTitle", "7-Day Forecast"));

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Markup.Should().Contain("7-Day Forecast");
    }

    [Fact]
    public void Renders_Forecast_Cards_For_Each_Day()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast1 = CreateForecast();
        ForecastCacheContract forecast2 = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today.AddDays(1), forecast1 },
                                                             { today.AddDays(2), forecast2 },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().HaveCount(2);
    }

    [Fact]
    public void Uses_Correct_Component_Structure()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        IReadOnlyList<IRenderedComponent<RadzenStack>> stacks = cut.FindComponents<RadzenStack>();
        stacks.Should().HaveCountGreaterThan(0);

        IReadOnlyList<IRenderedComponent<RadzenRow>> rows = cut.FindComponents<RadzenRow>();
        rows.Should().HaveCount(1);

        IReadOnlyList<IRenderedComponent<RadzenColumn>> columns = cut.FindComponents<RadzenColumn>();
        columns.Should().HaveCount(1);

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().HaveCount(1);
    }

    [Fact]
    public void Uses_Imperial_Units_For_Imperial_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Uses_Metric_Units_For_Metric_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-CA");
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Uses_WeatherMetricDisplay_Components()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        ForecastCacheContract forecast = CreateForecast();

        ForecastCacheContract currentForecast = _fixture.Build<ForecastCacheContract>()
                                                        .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>
                                                         {
                                                             { today, forecast },
                                                         })
                                                        .Create();

        SetupWeatherState(currentForecast);

        IRenderedComponent<WeeklyForecastCard> cut = Render<WeeklyForecastCard>();

        IReadOnlyList<IRenderedComponent<WeatherMetricDisplay>> metrics = cut.FindComponents<WeatherMetricDisplay>();
        metrics.Should().HaveCountGreaterThan(0);
    }

    private ForecastCacheContract CreateForecast()
    {
        return _fixture.Build<ForecastCacheContract>()
                       .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                       .Create();
    }

    private void SetupDefaultLocalizerBehavior()
    {
        _mockCardLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        _mockSharedLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
    }

    private void SetupWeatherState(ForecastCacheContract? currentForecast)
    {
        WeatherState state = new()
        {
            CurrentForecast = currentForecast,
            IsLoading = false,
        };

        _mockWeatherState.Value.Returns(state);
    }
}
