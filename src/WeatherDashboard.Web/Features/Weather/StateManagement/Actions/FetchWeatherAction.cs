namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

/// <summary>
///     Fluxor action to initiate fetching weather forecast data for a specific geographic location.
/// </summary>
/// <param name="Latitude">The latitude coordinate of the location.</param>
/// <param name="Longitude">The longitude coordinate of the location.</param>
internal sealed record FetchWeatherAction(double Latitude, double Longitude);
