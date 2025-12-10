namespace WeatherDashboard.Web.Features.Weather.StateManagement.Actions;

using Domain.Entities.Documents;

/// <summary>
///     Fluxor action dispatched when a location search completes successfully.
/// </summary>
/// <param name="Results">The collection of location search results matching the query.</param>
internal sealed record SearchLocationSuccessAction(IReadOnlyCollection<LocationDocument> Results);
