namespace WeatherDashboard.Web.Features.Weather.Components;

using System.Globalization;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.Extensions.Localization;
using StateManagement;
using UnitsNet;
using UnitsNet.Units;

/// <summary>
///     Code-behind for the CurrentHighlightsCard component that displays key weather metrics
///     such as humidity, UV index, wind, visibility, dew point, and surface pressure.
/// </summary>
public partial class CurrentHighlightsCard : FluxorComponent
{
    private readonly IStringLocalizer<CurrentHighlightsCard> _localizer;

    private readonly RegionInfo _regionInfo;

    private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

    private readonly IState<WeatherState> _weatherState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentHighlightsCard" /> class.
    /// </summary>
    /// <param name="localizer">The localizer for component-specific strings.</param>
    /// <param name="sharedLocalizer">The localizer for shared resource strings.</param>
    /// <param name="weatherState">The Fluxor weather state.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any parameter is <see langword="null" />.
    /// </exception>
    public CurrentHighlightsCard(IStringLocalizer<CurrentHighlightsCard> localizer,
                                 IStringLocalizer<SharedResource> sharedLocalizer,
                                 IState<WeatherState> weatherState)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _sharedLocalizer = sharedLocalizer ?? throw new ArgumentNullException(nameof(sharedLocalizer));
        _weatherState = weatherState ?? throw new ArgumentNullException(nameof(weatherState));

        _regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
    }

    /// <summary>
    ///     Gets the dew point temperature in degrees Fahrenheit.
    /// </summary>
    private double DewPoint => _weatherState.Value.CurrentForecast?.DewPoint ?? 0;

    /// <summary>
    ///     Gets the relative humidity percentage.
    /// </summary>
    private double Humidity => _weatherState.Value.CurrentForecast?.Humidity ?? 0;

    /// <summary>
    ///     Gets the surface pressure formatted in the appropriate unit (hectopascal for metric, inches of mercury for imperial).
    /// </summary>
    private string SurfacePressure =>
        Pressure.FromMillibars(_weatherState.Value.CurrentForecast?.SurfacePressure ?? 0)
                .ToUnit(_regionInfo.IsMetric ? PressureUnit.Hectopascal : PressureUnit.InchOfMercury)
                .ToString(CultureInfo.CurrentCulture);

    /// <summary>
    ///     Gets the UV index value.
    /// </summary>
    private double UvIndex => _weatherState.Value.CurrentForecast?.UvIndex ?? 0;

    /// <summary>
    ///     Gets the visibility distance formatted in the appropriate unit (kilometers for metric, miles for imperial).
    /// </summary>
    private string Visibility =>
        Length.FromFeet(_weatherState.Value.CurrentForecast?.Visibility ?? 0)
              .ToUnit(_regionInfo.IsMetric ? LengthUnit.Kilometer : LengthUnit.Mile)
              .ToString(CultureInfo.CurrentCulture);

    /// <summary>
    ///     Gets the wind direction in degrees.
    /// </summary>
    private double WindDirection => _weatherState.Value.CurrentForecast?.WindDirection ?? 0;

    /// <summary>
    ///     Gets the wind direction as a cardinal direction abbreviation (e.g., "N", "NE", "S", "SW").
    /// </summary>
    private string WindDirectionCardinal => WeatherHelpers.GetWindDirection(WindDirection);

    /// <summary>
    ///     Gets the weather icon class name representing the wind direction.
    /// </summary>
    private string WindDirectionIcon => WeatherIconMapper.GetWindDirectionIcon(WindDirectionCardinal);

    /// <summary>
    ///     Gets the wind gust speed in miles per hour.
    /// </summary>
    private double WindGusts => _weatherState.Value.CurrentForecast?.WindGusts ?? 0;

    /// <summary>
    ///     Gets the sustained wind speed in miles per hour.
    /// </summary>
    private double WindSpeed => _weatherState.Value.CurrentForecast?.WindSpeed ?? 0;

    /// <summary>
    ///     Gets the weather icon class name representing the Beaufort wind scale for the current wind speed.
    /// </summary>
    private string WindSpeedIcon => WeatherIconMapper.GetBeaufortWindScaleIcon(WindSpeed);
}
