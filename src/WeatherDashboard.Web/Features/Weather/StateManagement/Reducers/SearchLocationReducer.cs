namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="SearchLocationAction" /> by setting the location state to searching.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class SearchLocationReducer : Reducer<LocationState, SearchLocationAction>
{
    /// <summary>
    ///     Reduces the location state in response to <see cref="SearchLocationAction" />.
    /// </summary>
    /// <param name="state">The current location state.</param>
    /// <param name="action">The search location action containing the query.</param>
    /// <returns>
    ///     A new <see cref="LocationState" /> with <see cref="LocationState.SearchText" /> set to the query
    ///     and <see cref="LocationState.IsSearching" /> set to <see langword="true" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override LocationState Reduce(LocationState state, SearchLocationAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return state with
        {
            SearchText = action.Query,
            IsSearching = true,
        };
    }
}
