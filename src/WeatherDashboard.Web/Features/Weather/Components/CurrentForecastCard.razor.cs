namespace WeatherDashboard.Web.Features.Weather.Components;

using System.Globalization;
using Application.Contracts.Weather;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.Extensions.Localization;
using StateManagement;
using UnitsNet.Units;

/// <summary>
///     Code-behind for the CurrentForecastCard component that displays the current day's weather forecast,
///     including temperature, conditions, and sunrise/sunset times.
/// </summary>
public sealed partial class CurrentForecastCard : FluxorComponent
{
    private readonly RegionInfo _regionInfo;

    private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

    private readonly IState<WeatherState> _weatherState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentForecastCard" /> class.
    /// </summary>
    /// <param name="sharedLocalizer">The localizer for shared resource strings.</param>
    /// <param name="weatherState">The Fluxor weather state.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any parameter is <see langword="null" />.
    /// </exception>
    public CurrentForecastCard(IStringLocalizer<SharedResource> sharedLocalizer,
                               IState<WeatherState> weatherState)
    {
        _sharedLocalizer = sharedLocalizer ?? throw new ArgumentNullException(nameof(sharedLocalizer));
        _weatherState = weatherState ?? throw new ArgumentNullException(nameof(weatherState));

        _regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
    }

    /// <summary>
    ///     Gets the current day of the week formatted as "ddd, MMM d yyyy" (e.g., "Mon, Jan 1 2025").
    /// </summary>
    private string CurrentDayOfWeek
    {
        get
        {
            DateTimeOffset now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone);
            return now.ToString("ddd, MMM d yyyy", CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    ///     Gets the current weather forecast from the Fluxor state.
    /// </summary>
    private ForecastCacheContract? CurrentForecast => _weatherState.Value.CurrentForecast;

    /// <summary>
    ///     Gets the current time formatted with the time zone abbreviation suffix.
    /// </summary>
    private string CurrentTime =>
        WeatherHelpers.FormatTimeWithTimeZoneSuffix(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone), TimeZone);

    /// <summary>
    ///     Gets a value indicating whether it is currently nighttime based on sunrise and sunset times.
    /// </summary>
    private bool IsNightTime => WeatherHelpers.IsNightTime(CurrentForecast?.Sunrise, CurrentForecast?.Sunset, TimeZone);

    /// <summary>
    ///     Gets the sunrise time formatted with the time zone abbreviation suffix.
    /// </summary>
    private string SunriseTime => WeatherHelpers.FormatTimeWithTimeZoneSuffix(CurrentForecast?.Sunrise, TimeZone);

    /// <summary>
    ///     Gets the sunset time formatted with the time zone abbreviation suffix.
    /// </summary>
    private string SunsetTime => WeatherHelpers.FormatTimeWithTimeZoneSuffix(CurrentForecast?.Sunset, CurrentForecast?.TimeZone);

    /// <summary>
    ///     Gets the current temperature formatted in the appropriate unit (Celsius for metric, Fahrenheit for imperial).
    /// </summary>
    private string Temperature =>
        UnitsNet.Temperature.FromDegreesFahrenheit(_weatherState.Value.CurrentForecast?.Temperature ?? 0)
                .ToUnit(_regionInfo.IsMetric ? TemperatureUnit.DegreeCelsius : TemperatureUnit.DegreeFahrenheit)
                .ToString("F0", CultureInfo.CurrentCulture);

    /// <summary>
    ///     Gets the time zone for the current forecast location, or UTC if no forecast is loaded.
    /// </summary>
    private TimeZoneInfo TimeZone => _weatherState.Value.CurrentForecast?.TimeZone ?? TimeZoneInfo.Utc;
}
