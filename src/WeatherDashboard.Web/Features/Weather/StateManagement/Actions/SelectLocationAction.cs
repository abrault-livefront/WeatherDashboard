namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

using Domain.Entities.Documents;

/// <summary>
///     Fluxor action to initiate selection of a location for weather forecast retrieval.
/// </summary>
/// <param name="Location">The location document selected by the user.</param>
internal sealed record SelectLocationAction(LocationDocument Location);
