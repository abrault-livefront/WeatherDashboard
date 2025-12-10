namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="FetchWeatherAction" /> by setting the weather state to loading.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class FetchWeatherReducer : Reducer<WeatherState, FetchWeatherAction>
{
    /// <summary>
    ///     Reduces the weather state in response to <see cref="FetchWeatherAction" />.
    /// </summary>
    /// <param name="state">The current weather state.</param>
    /// <param name="action">The fetch weather action.</param>
    /// <returns>
    ///     A new <see cref="WeatherState" /> with <see cref="WeatherState.IsLoading" /> set to <see langword="true" />
    ///     and <see cref="WeatherState.CurrentForecast" /> set to <see langword="null" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override WeatherState Reduce(WeatherState state, FetchWeatherAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return new WeatherState
        {
            CurrentForecast = null,
            IsLoading = true,
        };
    }
}
