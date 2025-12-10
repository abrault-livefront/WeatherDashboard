namespace WeatherDashboard.Web.Features.Weather.StateManagement.Reducers;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor reducer that handles the <see cref="FetchWeatherSuccessAction" /> by updating the weather state
///     with the fetched forecast.
/// </summary>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Reducer instantiated by Fluxor")]
internal sealed class FetchWeatherSuccessReducer : Reducer<WeatherState, FetchWeatherSuccessAction>
{
    /// <summary>
    ///     Reduces the weather state in response to <see cref="FetchWeatherSuccessAction" />.
    /// </summary>
    /// <param name="state">The current weather state.</param>
    /// <param name="action">The fetch weather success action containing the forecast.</param>
    /// <returns>
    ///     A new <see cref="WeatherState" /> with <see cref="WeatherState.CurrentForecast" /> set to the fetched forecast
    ///     and <see cref="WeatherState.IsLoading" /> set to <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="state" /> or <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public override WeatherState Reduce(WeatherState state, FetchWeatherSuccessAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return new WeatherState
        {
            CurrentForecast = action.Forecast,
            IsLoading = false,
        };
    }
}
