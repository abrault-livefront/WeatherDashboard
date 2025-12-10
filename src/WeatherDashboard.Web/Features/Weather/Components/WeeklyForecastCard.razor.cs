namespace WeatherDashboard.Web.Features.Weather.Components;

using System.Globalization;
using Application.Contracts.Weather;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.Extensions.Localization;
using StateManagement;

/// <summary>
///     Code-behind for the WeeklyForecastCard component that displays a multi-day weather forecast.
/// </summary>
public partial class WeeklyForecastCard : FluxorComponent
{
    private readonly IStringLocalizer<WeeklyForecastCard> _localizer;

    private readonly RegionInfo _regionInfo;

    private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

    private readonly IState<WeatherState> _weatherState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WeeklyForecastCard" /> class.
    /// </summary>
    /// <param name="localizer">The localizer for component-specific strings.</param>
    /// <param name="sharedLocalizer">The localizer for shared resource strings.</param>
    /// <param name="weatherState">The Fluxor weather state.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any parameter is <see langword="null" />.
    /// </exception>
    public WeeklyForecastCard(IStringLocalizer<WeeklyForecastCard> localizer,
                              IStringLocalizer<SharedResource> sharedLocalizer,
                              IState<WeatherState> weatherState)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _sharedLocalizer = sharedLocalizer ?? throw new ArgumentNullException(nameof(sharedLocalizer));
        _weatherState = weatherState ?? throw new ArgumentNullException(nameof(weatherState));

        _regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
    }

    /// <summary>
    ///     Gets the collection of future weather forecasts indexed by date.
    /// </summary>
    private IDictionary<DateOnly, ForecastCacheContract> Forecasts =>
        _weatherState.Value.CurrentForecast?.FutureForecasts ?? new Dictionary<DateOnly, ForecastCacheContract>();
}
