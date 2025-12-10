namespace WeatherDashboard.Application.Contracts.Weather;

using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Weather.Enums;
using MessagePack;

/// <summary>
///     Represents a cached weather forecast data contract for serialization.
/// </summary>
[MessagePackObject]
public sealed class ForecastCacheContract
{
    /// <summary>
    ///     Gets or sets the dew point temperature in degrees Fahrenheit.
    /// </summary>
    [Key(1)]
    public double DewPoint { get; set; }

    /// <summary>
    ///     Gets or sets a collection of future forecast data indexed by date.
    /// </summary>
    [Key(0)]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO")]
    public IDictionary<DateOnly, ForecastCacheContract> FutureForecasts { get; set; } = new Dictionary<DateOnly, ForecastCacheContract>();

    /// <summary>
    ///     Gets or sets the relative humidity percentage.
    /// </summary>
    [Key(2)]
    public int Humidity { get; set; }

    /// <summary>
    ///     Gets or sets the latitude coordinate of the location.
    /// </summary>
    [Key(3)]
    public double Latitude { get; set; }

    /// <summary>
    ///     Gets or sets the longitude coordinate of the location.
    /// </summary>
    [Key(4)]
    public double Longitude { get; set; }

    /// <summary>
    ///     Gets or sets the sunrise time.
    /// </summary>
    [Key(5)]
    public DateTimeOffset Sunrise { get; set; }

    /// <summary>
    ///     Gets or sets the sunset time.
    /// </summary>
    [Key(6)]
    public DateTimeOffset Sunset { get; set; }

    /// <summary>
    ///     Gets or sets the surface atmospheric pressure in inHg.
    /// </summary>
    [Key(7)]
    public double SurfacePressure { get; set; }

    /// <summary>
    ///     Gets or sets the current temperature in degrees Fahrenheit.
    /// </summary>
    [Key(8)]
    public double Temperature { get; set; }

    /// <summary>
    ///     Gets or sets the time zone information for the location.
    /// </summary>
    [Key(9)]
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

    /// <summary>
    ///     Gets or sets the UV index value.
    /// </summary>
    [Key(10)]
    public double UvIndex { get; set; }

    /// <summary>
    ///     Gets or sets the visibility distance in miles.
    /// </summary>
    [Key(11)]
    public double Visibility { get; set; }

    /// <summary>
    ///     Gets or sets the weather condition code.
    /// </summary>
    [Key(12)]
    public WeatherCode WeatherCode { get; set; }

    /// <summary>
    ///     Gets or sets the wind direction in degrees.
    /// </summary>
    [Key(13)]
    public int WindDirection { get; set; }

    /// <summary>
    ///     Gets or sets the wind gust speed in miles per hour.
    /// </summary>
    [Key(14)]
    public double WindGusts { get; set; }

    /// <summary>
    ///     Gets or sets the wind speed in miles per hour.
    /// </summary>
    [Key(15)]
    public double WindSpeed { get; set; }
}
