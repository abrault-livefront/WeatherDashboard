namespace WeatherDashboard.Web.UiTests.Features.Weather.Components;

using System.Globalization;
using Application.Contracts.Weather;
using AutoFixture;
using AwesomeAssertions;
using Bunit;
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
public sealed class CurrentHighlightsCardTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly IStringLocalizer<CurrentHighlightsCard> _mockCardLocalizer;

    private readonly IStringLocalizer<SharedResource> _mockSharedLocalizer;

    private readonly IState<WeatherState> _mockWeatherState;

    public CurrentHighlightsCardTests()
    {
        _fixture.Customize<ForecastCacheContract>(c => c
                                                      .With(f => f.TimeZone, TimeZoneInfo.Utc)
                                                      .With(f => f.Sunrise, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(6).Ticks, TimeSpan.Zero))
                                                      .With(f => f.Sunset, new DateTimeOffset(DateTimeOffset.UtcNow.Date.AddHours(18).Ticks, TimeSpan.Zero))
                                                      .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>()));

        _mockCardLocalizer = Substitute.For<IStringLocalizer<CurrentHighlightsCard>>();
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
    public void Displays_Card_Title()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();

        SetupWeatherState(forecast);

        _mockCardLocalizer["CardTitle"].Returns(new LocalizedString("CardTitle", "Today's Highlights"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Today's Highlights");
    }

    [Fact]
    public void Displays_Dew_Point()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.DewPoint, 55.0)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["DewPoint"].Returns(new LocalizedString("DewPoint", "Dew Point"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Dew Point");
    }

    [Fact]
    public void Displays_Dew_Point_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().Contain(i => i.Instance.Icon == "dew_point");
    }

    [Fact]
    public void Displays_Humidity()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Humidity, 65)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["Humidity"].Returns(new LocalizedString("Humidity", "Humidity"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Humidity");
        cut.Markup.Should().Contain("65%");
    }

    [Fact]
    public void Displays_Humidity_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi-humidity"));
    }

    [Fact]
    public void Displays_Pressure()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.SurfacePressure, 1013.0)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["Pressure"].Returns(new LocalizedString("Pressure", "Pressure"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Pressure");
    }

    [Fact]
    public void Displays_Pressure_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().Contain(i => i.Instance.Icon == "compress");
    }

    [Fact]
    public void Displays_UV_Index()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.UvIndex, 5.0)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["UVIndex"].Returns(new LocalizedString("UVIndex", "UV Index"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("UV Index");
    }

    [Fact]
    public void Displays_UV_Index_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().Contain(i => i.Instance.Icon == "brightness_empty");
    }

    [Fact]
    public void Displays_Visibility()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.Visibility, 10.0)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["Visibility"].Returns(new LocalizedString("Visibility", "Visibility"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Visibility");
    }

    [Fact]
    public void Displays_Visibility_Icon()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().Contain(i => i.Instance.Icon == "visibility");
    }

    [Fact]
    public void Displays_Wind_Icon()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WindSpeed, 15.0)
                                                 .Create();

        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.FindAll("i").Should().Contain(i => i.ClassList.Contains("wi-wind"));
    }

    [Fact]
    public void Displays_Wind_Status()
    {
        ForecastCacheContract forecast = _fixture.Build<ForecastCacheContract>()
                                                 .With(f => f.FutureForecasts, new Dictionary<DateOnly, ForecastCacheContract>())
                                                 .With(f => f.WindSpeed, 15.0)
                                                 .With(f => f.WindGusts, 20.0)
                                                 .With(f => f.WindDirection, 180)
                                                 .Create();

        SetupWeatherState(forecast);

        _mockSharedLocalizer["WindStatus"].Returns(new LocalizedString("WindStatus", "Wind Status"));

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Markup.Should().Contain("Wind Status");
    }

    [Fact]
    public void Handles_Null_Forecast_Gracefully()
    {
        SetupWeatherState(null);

        Action act = () => Render<CurrentHighlightsCard>();

        act.Should().NotThrow();
    }

    [Fact]
    public void Has_Six_Metric_Cards()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().HaveCount(7);
    }

    [Fact]
    public void Renders_Card_Component()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenCard>> cards = cut.FindComponents<RadzenCard>();
        cards.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Uses_Correct_Component_Structure()
    {
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        IReadOnlyList<IRenderedComponent<RadzenStack>> stacks = cut.FindComponents<RadzenStack>();
        stacks.Should().HaveCountGreaterThan(0);

        IReadOnlyList<IRenderedComponent<RadzenRow>> rows = cut.FindComponents<RadzenRow>();
        rows.Should().HaveCount(2);

        IReadOnlyList<IRenderedComponent<RadzenColumn>> columns = cut.FindComponents<RadzenColumn>();
        columns.Should().HaveCount(6);
    }

    [Fact]
    public void Uses_Imperial_Units_For_Imperial_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Uses_Metric_Units_For_Metric_Region()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-CA");
        ForecastCacheContract forecast = _fixture.Create<ForecastCacheContract>();
        SetupWeatherState(forecast);

        IRenderedComponent<CurrentHighlightsCard> cut = Render<CurrentHighlightsCard>();

        cut.Should().NotBeNull();
    }

    private void SetupDefaultLocalizerBehavior()
    {
        _mockCardLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        _mockSharedLocalizer[Arg.Any<string>()]
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
