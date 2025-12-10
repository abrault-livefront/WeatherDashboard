namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Reducers;

using Application.Contracts.Weather;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Web.Features.Weather.StateManagement;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Reducers;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Weather")]
[Trait("Component", "Reducer")]
[Trait("Speed", "Fast")]
public sealed class FetchWeatherSuccessReducerTests
{
    [Theory]
    [AutoData]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState(double latitude, double longitude, double temperature)
    {
        FetchWeatherSuccessReducer reducer = new();

        ForecastCacheContract contract = new()
        {
            Latitude = latitude,
            Longitude = longitude,
            Temperature = temperature,
        };

        WeatherState state = new()
        {
            CurrentForecast = contract,
            IsLoading = false,
        };

        FetchWeatherSuccessAction action = new(contract);

        WeatherState result = reducer.Reduce(state, action);

        result.CurrentForecast.Should().Be(contract);
        result.IsLoading.Should().BeFalse();
    }
}
