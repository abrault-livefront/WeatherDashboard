namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Reducers;

using AutoFixture.Xunit3;
using AwesomeAssertions;
using Web.Features.Weather.StateManagement;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Reducers;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Search")]
[Trait("Component", "Reducer")]
[Trait("Speed", "Fast")]
public sealed class SearchLocationReducerTests
{
    [Theory]
    [AutoData]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState(string query)
    {
        SearchLocationReducer reducer = new();

        LocationState state = new()
        {
            SearchText = query,
            IsSearching = true,
        };

        SearchLocationAction action = new(query);

        LocationState result = reducer.Reduce(state, action);

        result.SearchText.Should().Be(query);
        result.IsSearching.Should().BeTrue();
    }
}
