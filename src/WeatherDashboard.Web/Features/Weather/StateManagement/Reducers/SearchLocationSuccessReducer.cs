namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="SearchLocationSuccessAction" /> by updating the location state
///     with search results.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class SearchLocationSuccessReducer : Reducer<LocationState, SearchLocationSuccessAction>
{
    /// <summary>
    ///     Reduces the location state in response to <see cref="SearchLocationSuccessAction" />.
    /// </summary>
    /// <param name="state">The current location state.</param>
    /// <param name="action">The search location success action containing the results.</param>
    /// <returns>
    ///     A new <see cref="LocationState" /> with <see cref="LocationState.SearchResults" /> set to the search results
    ///     and <see cref="LocationState.IsSearching" /> set to <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override LocationState Reduce(LocationState state, SearchLocationSuccessAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return state with
        {
            SearchResults = action.Results,
            IsSearching = false,
        };
    }
}
