namespace WeatherDashboard.Web.UiTests.Features.Weather;

using AngleSharp.Dom;
using AutoFixture;
using AwesomeAssertions;
using Bunit;
using Configuration;
using Domain.Entities.Documents;
using Fluxor;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Radzen;
using Radzen.Blazor;
using Web.Features.Weather;
using Web.Features.Weather.Components;
using Web.Features.Weather.StateManagement;
using HomeStrings = Localizations.Features.Weather.Home;

[Trait("Category", "UI")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "BlazorComponent")]
[Trait("Speed", "Slow")]
public sealed class HomeTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly IStringLocalizer<Home> _mockLocalizer;

    private readonly IState<LocationState> _mockLocationState;

    private readonly IState<WeatherState> _mockWeatherState;

    public HomeTests()
    {
        IDispatcher mockDispatcher = Substitute.For<IDispatcher>();

        _mockLocalizer = Substitute.For<IStringLocalizer<Home>>();
        _mockLocationState = Substitute.For<IState<LocationState>>();
        _mockWeatherState = Substitute.For<IState<WeatherState>>();

        IWebHostEnvironment mockWebHostEnvironment = Substitute.For<IWebHostEnvironment>();

        LocalStorageSettings localStorageSettings = new()
        {
            Prefix = "WeatherDashboard.Web",
            IncludeEnvironmentInName = true,
        };
        IOptions<LocalStorageSettings> mockLocalStorageSettings = Options.Create(localStorageSettings);

        mockWebHostEnvironment.EnvironmentName.Returns("Development");

        IStore mockStore = Substitute.For<IStore>();
        IActionSubscriber mockActionSubscriber = Substitute.For<IActionSubscriber>();

        // Mock localizers for child components
        IStringLocalizer<LocationSearchField> mockLocationSearchLocalizer = Substitute.For<IStringLocalizer<LocationSearchField>>();
        mockLocationSearchLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        IStringLocalizer<SharedResource> mockSharedLocalizer = Substitute.For<IStringLocalizer<SharedResource>>();
        mockSharedLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        IStringLocalizer<CurrentForecastCard> mockCurrentForecastLocalizer = Substitute.For<IStringLocalizer<CurrentForecastCard>>();
        mockCurrentForecastLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        IStringLocalizer<CurrentHighlightsCard> mockCurrentHighlightsLocalizer = Substitute.For<IStringLocalizer<CurrentHighlightsCard>>();
        mockCurrentHighlightsLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        IStringLocalizer<WeeklyForecastCard> mockWeeklyForecastLocalizer = Substitute.For<IStringLocalizer<WeeklyForecastCard>>();
        mockWeeklyForecastLocalizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));

        Services.AddSingleton(mockStore);
        Services.AddSingleton(mockDispatcher);
        Services.AddSingleton(mockActionSubscriber);
        Services.AddSingleton(_mockLocalizer);
        Services.AddSingleton(mockLocalStorageSettings);
        Services.AddSingleton(_mockLocationState);
        Services.AddSingleton(_mockWeatherState);
        Services.AddSingleton(mockWebHostEnvironment);
        Services.AddSingleton(mockLocationSearchLocalizer);
        Services.AddSingleton(mockSharedLocalizer);
        Services.AddSingleton(mockCurrentForecastLocalizer);
        Services.AddSingleton(mockCurrentHighlightsLocalizer);
        Services.AddSingleton(mockWeeklyForecastLocalizer);
        Services.AddDataProtection();
        Services.AddSingleton(sp => new ProtectedLocalStorage(JSInterop.JSRuntime, sp.GetRequiredService<IDataProtectionProvider>()));

        // Add Radzen services
        Services.AddRadzenComponents();

        // Configure JSInterop for localStorage calls
        JSInterop.Mode = JSRuntimeMode.Loose;

        SetupDefaultLocalizerBehavior();
        SetupDefaultLocationState(null);
        SetupDefaultWeatherState(false);
    }

    [Fact]
    public void Displays_Current_Location_When_Available()
    {
        LocationDocument location = _fixture.Build<LocationDocument>()
                                            .With(l => l.Locality, "Toronto")
                                            .With(l => l.ProvinceCode, "ON")
                                            .Create();

        SetupDefaultLocationState(location);

        IRenderedComponent<Home> cut = Render<Home>();

        cut.Markup.Should().Contain("Toronto");
        cut.Markup.Should().Contain("ON");
    }

    [Fact]
    public void Displays_Location_Icon()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Markup.Should().Contain("location_on");
    }

    [Fact]
    public void Displays_No_Location_Message_When_Location_Is_Null()
    {
        _mockLocalizer[nameof(HomeStrings.NoLocation)].Returns(new LocalizedString(nameof(HomeStrings.NoLocation), "No Location Selected"));
        SetupDefaultLocationState(null);

        IRenderedComponent<Home> cut = Render<Home>();

        cut.Markup.Should().Contain("No Location Selected");
    }

    [Fact]
    public void Displays_Page_Title()
    {
        _mockLocalizer[nameof(HomeStrings.PageTitle)].Returns(new LocalizedString(nameof(HomeStrings.PageTitle), "Weather Dashboard"));

        IRenderedComponent<Home> cut = Render<Home>();

        cut.Markup.Should().Contain("Weather Dashboard");
    }

    [Theory]
    [InlineData("New York", "NY")]
    [InlineData("Los Angeles", "CA")]
    [InlineData("Chicago", "IL")]
    public void Displays_Various_Locations_Correctly(string locality, string provinceCode)
    {
        LocationDocument location = _fixture.Build<LocationDocument>()
                                            .With(l => l.Locality, locality)
                                            .With(l => l.ProvinceCode, provinceCode)
                                            .Create();

        SetupDefaultLocationState(location);

        IRenderedComponent<Home> cut = Render<Home>();

        cut.Markup.Should().Contain(locality);
        cut.Markup.Should().Contain(provinceCode);
    }

    [Fact]
    public void Equal_Height_CSS_Classes_Are_Applied()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IEnumerable<IElement> equalHeightCols = cut.FindAll(".equal-height--col");
        equalHeightCols.Should().HaveCount(2);

        IEnumerable<IElement> equalHeightCards = cut.FindAll(".equal-height--card");
        equalHeightCards.Should().HaveCount(2);
    }

    [Fact]
    public void First_Row_Contains_Location_And_Search_Field()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<RadzenRow>> rows = cut.FindComponents<RadzenRow>();
        IRenderedComponent<RadzenRow> firstRow = rows[0];

        // Should contain location icon and search field stub
        firstRow.Markup.Should().Contain("location_on");
    }

    [Fact]
    public void Has_Correct_Number_Of_RadzenRows()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<RadzenRow>> rows = cut.FindComponents<RadzenRow>();
        rows.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Has_Screen_Reader_Only_H1_Title()
    {
        _mockLocalizer[nameof(HomeStrings.PageTitle)].Returns(new LocalizedString(nameof(HomeStrings.PageTitle), "Weather Dashboard"));

        IRenderedComponent<Home> cut = Render<Home>();

        IElement h1 = cut.Find("h1");
        h1.ClassList.Should().Contain("sr-only");
        h1.TextContent.Should().Contain("Weather Dashboard");
    }

    [Fact]
    public void Location_Display_Has_Bold_Font_Weight()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IElement locationSpan = cut.Find("span.font-weight-bold");
        locationSpan.ClassList.Should().Contain("font-weight-bold");
    }

    [Fact]
    public void Location_Display_Shows_Locality_And_Province_Code_Together()
    {
        LocationDocument location = _fixture.Build<LocationDocument>()
                                            .With(l => l.Locality, "Seattle")
                                            .With(l => l.ProvinceCode, "WA")
                                            .Create();

        SetupDefaultLocationState(location);

        IRenderedComponent<Home> cut = Render<Home>();

        IElement locationSpan = cut.Find("span.font-weight-bold");
        locationSpan.TextContent.Should().Contain("Seattle");
        locationSpan.TextContent.Should().Contain("WA");
    }

    [Fact]
    public void Renders_CurrentForecastCard_Component()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<CurrentForecastCard>> components = cut.FindComponents<CurrentForecastCard>();
        components.Should().HaveCount(1);
    }

    [Fact]
    public void Renders_CurrentHighlightsCard_Component()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<CurrentHighlightsCard>> components = cut.FindComponents<CurrentHighlightsCard>();
        components.Should().HaveCount(1);
    }

    [Fact]
    public void Renders_LocationSearchField_Component()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<LocationSearchField>> components = cut.FindComponents<LocationSearchField>();
        components.Should().HaveCount(1);
    }

    [Fact]
    public void Renders_RadzenAppearanceToggle()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<RadzenAppearanceToggle>> toggles = cut.FindComponents<RadzenAppearanceToggle>();
        toggles.Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Successfully()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Renders_WeeklyForecastCard_Component()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<WeeklyForecastCard>> components = cut.FindComponents<WeeklyForecastCard>();
        components.Should().HaveCount(1);
    }

    [Fact]
    public void Second_Row_Contains_Forecast_Cards()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        // Verify both forecast components are present
        cut.FindComponents<CurrentForecastCard>().Should().HaveCount(1);
        cut.FindComponents<CurrentHighlightsCard>().Should().HaveCount(1);
    }

    [Fact]
    public void Third_Row_Contains_Weekly_Forecast()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        // Verify weekly forecast component is present
        cut.FindComponents<WeeklyForecastCard>().Should().HaveCount(1);
    }

    [Fact]
    public void Uses_RadzenRow_And_RadzenColumn_Layout()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<RadzenRow>> rows = cut.FindComponents<RadzenRow>();
        rows.Should().HaveCountGreaterThan(0);

        IReadOnlyList<IRenderedComponent<RadzenColumn>> columns = cut.FindComponents<RadzenColumn>();
        columns.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Uses_RadzenStack_Layout()
    {
        IRenderedComponent<Home> cut = Render<Home>();

        IReadOnlyList<IRenderedComponent<RadzenStack>> stacks = cut.FindComponents<RadzenStack>();
        stacks.Should().HaveCountGreaterThan(0);
    }

    private void SetupDefaultLocalizerBehavior()
    {
        _mockLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
    }

    private void SetupDefaultLocationState(LocationDocument? location)
    {
        LocationState state = new()
        {
            CurrentLocation = location,
            IsSearching = false,
            SearchResults = [],
            SearchText = string.Empty,
        };

        _mockLocationState.Value.Returns(state);
    }

    private void SetupDefaultWeatherState(bool isLoading)
    {
        WeatherState state = new()
        {
            CurrentForecast = null,
            IsLoading = isLoading,
        };

        _mockWeatherState.Value.Returns(state);
    }
}
