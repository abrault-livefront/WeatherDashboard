namespace WeatherDashboard.Web.UiTests.Features.Weather.Components;

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
using Radzen.Blazor;
using Web.Features.Weather.Components;
using Web.Features.Weather.StateManagement;

[Trait("Category", "UI")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "BlazorComponent")]
[Trait("Speed", "Slow")]
public sealed class LocationSearchFieldTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly IStringLocalizer<LocationSearchField> _mockLocalizer;

    private readonly IState<LocationState> _mockLocationState;

    public LocationSearchFieldTests()
    {
        IDispatcher mockDispatcher = Substitute.For<IDispatcher>();

        _mockLocalizer = Substitute.For<IStringLocalizer<LocationSearchField>>();
        _mockLocationState = Substitute.For<IState<LocationState>>();

        IStore mockStore = Substitute.For<IStore>();
        IActionSubscriber mockActionSubscriber = Substitute.For<IActionSubscriber>();

        IWebHostEnvironment mockWebHostEnvironment = Substitute.For<IWebHostEnvironment>();
        mockWebHostEnvironment.EnvironmentName.Returns("Development");

        LocalStorageSettings localStorageSettings = new()
        {
            Prefix = "WeatherDashboard.Web",
            IncludeEnvironmentInName = true,
        };

        IOptions<LocalStorageSettings> mockLocalStorageSettings = Substitute.For<IOptions<LocalStorageSettings>>();
        mockLocalStorageSettings.Value.Returns(localStorageSettings);

        Services.AddSingleton(mockStore);
        Services.AddSingleton(mockDispatcher);
        Services.AddSingleton(mockActionSubscriber);
        Services.AddSingleton(_mockLocalizer);
        Services.AddSingleton(_mockLocationState);
        Services.AddSingleton(mockWebHostEnvironment);
        Services.AddSingleton(mockLocalStorageSettings);
        Services.AddDataProtection();
        Services.AddSingleton(sp => new ProtectedLocalStorage(JSInterop.JSRuntime, sp.GetRequiredService<IDataProtectionProvider>()));

        SetupDefaultLocalizerBehavior();
    }

    [Fact]
    public void Binds_Search_Results_To_AutoComplete()
    {
        LocationDocument location = CreateLocation("New York", "New York");
        SetupLocationState([location,]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.Data.As<IEnumerable<LocationDocument>>().Should().Contain(location);
    }

    [Fact]
    public void Displays_Empty_Results_When_No_Matches()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.Data.As<IEnumerable<LocationDocument>>().Should().BeEmpty();
    }

    [Fact]
    public void Displays_Multiple_Search_Results()
    {
        LocationDocument location1 = CreateLocation("New York", "New York");
        LocationDocument location2 = CreateLocation("Los Angeles", "California");
        SetupLocationState([location1, location2,]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.Data.As<IEnumerable<LocationDocument>>().Should().HaveCount(2);
    }

    [Fact]
    public void Displays_Placeholder_Text()
    {
        SetupLocationState([]);
        _mockLocalizer["SearchCityOrZipCodePlaceholder"]
           .Returns(new LocalizedString("SearchCityOrZipCodePlaceholder", "Search city or zip code"));

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.Placeholder.Should().Be("Search city or zip code");
    }

    [Fact]
    public void Displays_Search_Icon()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().Contain(i => i.Instance.Icon == "search");
    }

    [Fact]
    public void Handles_Null_Search_Results_Gracefully()
    {
        LocationState state = new()
        {
            SearchResults = [],
        };
        _mockLocationState.Value.Returns(state);

        Action act = () => Render<LocationSearchField>();

        act.Should().NotThrow();
    }

    [Fact]
    public void Has_TabIndex_Zero()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.TabIndex.Should().Be(0);
    }

    [Fact]
    public void Renders_AutoComplete_Component()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();
        autoComplete.Should().NotBeNull();
    }

    [Fact]
    public void Renders_Label_Component()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenLabel> label = cut.FindComponent<RadzenLabel>();
        label.Should().NotBeNull();
    }

    [Fact]
    public void Renders_Label_For_Accessibility()
    {
        SetupLocationState([]);
        _mockLocalizer["SearchCityOrZipCodePlaceholder"]
           .Returns(new LocalizedString("SearchCityOrZipCodePlaceholder", "Search city or zip code"));

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenLabel> label = cut.FindComponent<RadzenLabel>();
        label.Instance.Text.Should().Be("Search city or zip code");
        label.Instance.Component.Should().Be("LocationSearch");
    }

    [Fact]
    public void Uses_Correct_Component_Structure()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IReadOnlyList<IRenderedComponent<RadzenLabel>> labels = cut.FindComponents<RadzenLabel>();
        labels.Should().HaveCount(1);

        IReadOnlyList<IRenderedComponent<RadzenAutoComplete>> autoCompletes =
            cut.FindComponents<RadzenAutoComplete>();

        autoCompletes.Should().HaveCount(1);

        IReadOnlyList<IRenderedComponent<RadzenIcon>> icons = cut.FindComponents<RadzenIcon>();
        icons.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Uses_TextProperty_Locality()
    {
        SetupLocationState([]);

        IRenderedComponent<LocationSearchField> cut = Render<LocationSearchField>();

        IRenderedComponent<RadzenAutoComplete> autoComplete =
            cut.FindComponent<RadzenAutoComplete>();

        autoComplete.Instance.TextProperty.Should().Be("Locality");
    }

    private LocationDocument CreateLocation(string locality, string province)
    {
        return new LocationDocument(
            Guid.NewGuid(),
            locality,
            province,
            province[..2].ToUpperInvariant(),
            ["12345",],
            _fixture.Create<double>(),
            _fixture.Create<double>());
    }

    private void SetupDefaultLocalizerBehavior()
    {
        _mockLocalizer[Arg.Any<string>()]
           .Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
    }

    private void SetupLocationState(IReadOnlyCollection<LocationDocument> searchResults)
    {
        LocationState state = new()
        {
            SearchResults = searchResults,
            IsSearching = false,
            SearchText = string.Empty,
        };

        _mockLocationState.Value.Returns(state);
    }
}
