namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Reducers;

using Application.Contracts.Weather;
using AutoFixture;
using AwesomeAssertions;
using Web.Features.Weather.StateManagement;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Reducers;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "Reducer")]
[Trait("Speed", "Fast")]
public sealed class FetchWeatherFailureReducerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState()
    {
        FetchWeatherFailureReducer reducer = new();

        WeatherState state = new()
        {
            CurrentForecast = new ForecastCacheContract
            {
                Latitude = _fixture.Create<double>(),
                Longitude = _fixture.Create<double>(),
                Temperature = _fixture.Create<double>(),
            },
            IsLoading = true,
        };

        FetchWeatherFailureAction action = new();

        WeatherState result = reducer.Reduce(state, action);

        result.CurrentForecast.Should().BeNull();
        result.IsLoading.Should().BeFalse();
    }
}
