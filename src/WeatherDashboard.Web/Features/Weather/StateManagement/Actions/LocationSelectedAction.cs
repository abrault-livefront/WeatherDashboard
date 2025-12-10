namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

using Domain.Entities.Documents;

/// <summary>
///     Fluxor action dispatched after a location has been successfully selected.
/// </summary>
/// <param name="Location">The location that was selected.</param>
internal sealed record LocationSelectedAction(LocationDocument Location);
