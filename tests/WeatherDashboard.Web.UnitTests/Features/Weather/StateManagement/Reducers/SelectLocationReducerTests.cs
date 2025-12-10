namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Reducers;

using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Entities.Documents;
using Web.Features.Weather.StateManagement;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Reducers;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Search")]
[Trait("Component", "Reducer")]
[Trait("Speed", "Fast")]
public sealed class SelectLocationReducerTests
{
    [Theory]
    [AutoData]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState(
        IReadOnlyCollection<LocationDocument> searchResults,
        string searchText,
        LocationDocument location)
    {
        SelectLocationReducer reducer = new();

        LocationState state = new()
        {
            SearchText = searchText,
            CurrentLocation = location,
            IsSearching = false,
            SearchResults = searchResults,
        };

        SelectLocationAction action = new(location);

        LocationState result = reducer.Reduce(state, action);

        result.SearchResults.Should().BeEmpty();
        result.IsSearching.Should().BeFalse();
        result.SearchText.Should().BeEmpty();
        result.CurrentLocation.Should().Be(location);
    }
}
