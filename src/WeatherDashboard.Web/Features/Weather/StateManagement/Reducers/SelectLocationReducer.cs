namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="SelectLocationAction" /> by updating the current location
///     and clearing search state.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class SelectLocationReducer : Reducer<LocationState, SelectLocationAction>
{
    /// <summary>
    ///     Reduces the location state in response to <see cref="SelectLocationAction" />.
    /// </summary>
    /// <param name="state">The current location state.</param>
    /// <param name="action">The select location action containing the selected location.</param>
    /// <returns>
    ///     A new <see cref="LocationState" /> with <see cref="LocationState.CurrentLocation" /> set to the selected location,
    ///     <see cref="LocationState.SearchResults" /> cleared, and <see cref="LocationState.SearchText" /> reset to empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override LocationState Reduce(LocationState state, SelectLocationAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return state with
        {
            CurrentLocation = action.Location,
            SearchResults = [],
            SearchText = string.Empty,
        };
    }
}
