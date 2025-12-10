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
public sealed class FetchWeatherReducerTests
{
    [Theory]
    [AutoData]
    public void Reduce_WithValidStateAndAction_ReturnsCorrectState(double latitude, double longitude, double temperature)
    {
        FetchWeatherReducer reducer = new();

        WeatherState state = new()
        {
            CurrentForecast = new ForecastCacheContract
            {
                Latitude = latitude,
                Longitude = longitude,
                Temperature = temperature,
            },
            IsLoading = true,
        };

        FetchWeatherAction action = new(latitude, longitude);

        WeatherState result = reducer.Reduce(state, action);

        result.CurrentForecast.Should().BeNull();
        result.IsLoading.Should().BeTrue();
    }
}
