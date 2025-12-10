namespace WeatherDashboard.Web.Features.Weather.StateManagement.Effects;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Fluxor;

/// <summary>
///     Fluxor effect that handles location selection and triggers weather forecast retrieval.
/// </summary>
/// <remarks>
///     This effect responds to <see cref="SelectLocationAction" /> by dispatching
///     <see cref="LocationSelectedAction" /> to update the location state, and then dispatching
///     <see cref="FetchWeatherAction" /> to fetch weather data for the selected location.
/// </remarks>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Effect instantiated by Fluxor")]
internal sealed class SelectLocationEffect : Effect<SelectLocationAction>
{
    /// <summary>
    ///     Handles the <see cref="SelectLocationAction" /> by dispatching location selection
    ///     and weather fetch actions.
    /// </summary>
    /// <param name="action">The action containing the selected location.</param>
    /// <param name="dispatcher">The Fluxor dispatcher used to dispatch follow-up actions.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="action" /> or <paramref name="dispatcher" /> is <see langword="null" />.
    /// </exception>
    public override Task HandleAsync(SelectLocationAction action, IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(dispatcher);

        dispatcher.Dispatch(new LocationSelectedAction(action.Location));
        dispatcher.Dispatch(new FetchWeatherAction(action.Location.Latitude, action.Location.Longitude));

        return Task.CompletedTask;
    }
}
