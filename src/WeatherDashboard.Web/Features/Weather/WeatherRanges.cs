namespace WeatherDashboard.Web.Features.Weather;

using Domain.Collections;
using Domain.ValueObjects;
using SharedStrings = Localizations.SharedResource;

/// <summary>
///     Provides predefined value ranges for various weather metrics and their corresponding classifications.
/// </summary>
internal static class WeatherRanges
{
    /// <summary>
    ///     Maps humidity percentage ranges to descriptive classifications.
    ///     Classifications range from "Very Low" (0-25.9999%) to "Very High" (71-100%).
    /// </summary>
    public static readonly ValueRangeMap<double, string> HumidityValueRanges = new()
    {
        { new ValueRange<double>(0, 25.9999), nameof(SharedStrings.Weather_Humidity_VeryLow) },
        { new ValueRange<double>(26, 30.9999), nameof(SharedStrings.Weather_Humidity_Low) },
        { new ValueRange<double>(31, 40.9999), nameof(SharedStrings.Weather_Humidity_Comfortable) },
        { new ValueRange<double>(41, 60.9999), nameof(SharedStrings.Weather_Humidity_Moderate) },
        { new ValueRange<double>(61, 70.9999), nameof(SharedStrings.Weather_Humidity_High) },
        { new ValueRange<double>(71, 100), nameof(SharedStrings.Weather_Humidity_VeryHigh) },
    };

    /// <summary>
    ///     Maps UV index values to standard UV exposure risk categories.
    ///     Classifications range from "Low" (0-2.9999) to "Extreme" (11+).
    /// </summary>
    public static readonly ValueRangeMap<double, string> UvIndexValueRanges = new()
    {
        { new ValueRange<double>(0, 2.9999), nameof(SharedStrings.Weather_UvIndex_Low) },
        { new ValueRange<double>(3, 5.9999), nameof(SharedStrings.Weather_UvIndex_Moderate) },
        { new ValueRange<double>(6, 7.9999), nameof(SharedStrings.Weather_UvIndex_High) },
        { new ValueRange<double>(8, 10.9999), nameof(SharedStrings.Weather_UvIndex_VeryHigh) },
        { new ValueRange<double>(11, int.MaxValue), nameof(SharedStrings.Weather_UvIndex_Extreme) },
    };
}
