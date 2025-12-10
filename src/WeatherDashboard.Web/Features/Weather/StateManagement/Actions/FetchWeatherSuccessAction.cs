namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

using Application.Contracts.Weather;

/// <summary>
///     Fluxor action dispatched when weather forecast data has been successfully fetched.
/// </summary>
/// <param name="Forecast">The weather forecast data retrieved from the service.</param>
internal sealed record FetchWeatherSuccessAction(ForecastCacheContract Forecast);
