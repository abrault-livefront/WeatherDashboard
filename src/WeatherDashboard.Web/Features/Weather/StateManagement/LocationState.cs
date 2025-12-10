namespace WeatherDashboard.Web.Features.Weather.StateManagement;

using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Documents;
using Fluxor;
using JetBrains.Annotations;

/// <summary>
///     Represents the Fluxor state for location search and selection.
/// </summary>
/// <remarks>
///     This state is managed by Fluxor and contains the current location, search query,
///     search results, and search status. Reducers update this state in response to location-related actions.
/// </remarks>
[UsedImplicitly]
[FeatureState]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
public sealed record LocationState
{
    /// <summary>
    ///     Gets or initializes the currently selected location.
    ///     This is <see langword="null" /> when no location has been selected.
    /// </summary>
    public LocationDocument? CurrentLocation { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether a location search is currently in progress.
    /// </summary>
    public bool IsSearching { get; init; }

    /// <summary>
    ///     Gets or initializes the collection of location search results.
    ///     This is an empty collection when no search has been performed or when the search returns no results.
    /// </summary>
    public IReadOnlyCollection<LocationDocument> SearchResults { get; init; } = [];

    /// <summary>
    ///     Gets or initializes the current search query text.
    ///     This is an empty string when no search has been initiated.
    /// </summary>
    public string SearchText { get; init; } = string.Empty;
}
