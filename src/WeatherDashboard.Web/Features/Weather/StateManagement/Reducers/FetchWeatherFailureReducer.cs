namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="FetchWeatherFailureAction" /> by clearing the forecast
///     and stopping the loading state.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class FetchWeatherFailureReducer : Reducer<WeatherState, FetchWeatherFailureAction>
{
    /// <summary>
    ///     Reduces the weather state in response to <see cref="FetchWeatherFailureAction" />.
    /// </summary>
    /// <param name="state">The current weather state.</param>
    /// <param name="action">The fetch weather failure action.</param>
    /// <returns>
    ///     A new <see cref="WeatherState" /> with <see cref="WeatherState.CurrentForecast" /> set to <see langword="null" />
    ///     and <see cref="WeatherState.IsLoading" /> set to <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override WeatherState Reduce(WeatherState state, FetchWeatherFailureAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return new WeatherState
        {
            CurrentForecast = null,
            IsLoading = false,
        };
    }
}
