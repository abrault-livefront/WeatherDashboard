namespace WeatherDashboard.Domain.Entities.Weather;

using System.Diagnostics.CodeAnalysis;
using Enums;

/// <summary>
///     Model representing a weather forecast.
/// </summary>
public sealed class Forecast
{
    /// <summary>
    ///     Gets or sets the dew point temperature.
    /// </summary>
    public double DewPoint { get; set; }

    /// <summary>
    ///     Gets or sets the list of future daily forecasts.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public IDictionary<DateOnly, Forecast> FutureForecasts { get; set; } = new Dictionary<DateOnly, Forecast>(0);

    /// <summary>
    ///     Gets or sets the relative humidity as a percentage.
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    ///     Gets or sets the latitude for the forecast.
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    ///     Gets or sets the longitude for the forecast.
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    ///     Gets or sets the sunrise time for the forecast.
    /// </summary>
    public DateTimeOffset Sunrise { get; set; }

    /// <summary>
    ///     Gets or sets the sunset time for the forecast.
    /// </summary>
    public DateTimeOffset Sunset { get; set; }

    /// <summary>
    ///     Gets or sets the mean surface pressure.
    /// </summary>
    public double SurfacePressure { get; set; }

    /// <summary>
    ///     Gets or sets the temperature for the forecast.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    ///     Gets or sets the timezone for the forecast.
    /// </summary>
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;

    /// <summary>
    ///     Gets or sets the UV index for the forecast.
    /// </summary>
    public double UvIndex { get; set; }

    /// <summary>
    ///     Gets or sets the visibility for the forecast.
    /// </summary>
    public double Visibility { get; set; }

    /// <summary>
    ///     Gets or sets the WMO weather code for the forecast.
    /// </summary>
    public WeatherCode WeatherCode { get; set; }

    /// <summary>
    ///     Gets or sets the wind direction for the forecast.
    /// </summary>
    public int WindDirection { get; set; }

    /// <summary>
    ///     Gets or sets the wind gust for the forecast.
    /// </summary>
    public double WindGusts { get; set; }

    /// <summary>
    ///     Gets or sets the wind speed for the forecast.
    /// </summary>
    public double WindSpeed { get; set; }
}
