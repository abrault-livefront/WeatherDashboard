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
public sealed class SearchLocationSuccessReducerTests
{
    [Theory]
    [AutoData]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState(
        IReadOnlyCollection<LocationDocument> searchResults,
        string searchText,
        LocationDocument location)
    {
        SearchLocationSuccessReducer reducer = new();

        LocationState state = new()
        {
            SearchText = searchText,
            CurrentLocation = location,
            IsSearching = true,
            SearchResults = [],
        };

        SearchLocationSuccessAction action = new(searchResults);

        LocationState result = reducer.Reduce(state, action);

        result.SearchResults.Should().BeEquivalentTo(searchResults);
        result.IsSearching.Should().BeFalse();
        result.SearchText.Should().Be(searchText);
        result.CurrentLocation.Should().Be(location);
    }
}
