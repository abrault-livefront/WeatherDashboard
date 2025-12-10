namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

/// <summary>
///     Fluxor action to initiate a search for locations matching a query string.
/// </summary>
/// <param name="Query">The search query text entered by the user.</param>
internal sealed record SearchLocationAction(string Query);
