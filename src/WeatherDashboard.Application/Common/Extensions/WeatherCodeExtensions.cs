namespace WeatherDashboard.Application.Common.Extensions;

using Domain.Entities.Weather.Enums;

/// <summary>
/// Provides extension methods for the <see cref="WeatherCode"/> enumeration.
/// </summary>
public static class WeatherCodeExtensions
{
    /// <summary>
    /// Converts a <see cref="WeatherCode"/> value to its resource key string.
    /// </summary>
    /// <param name="weatherCode">The weather code value.</param>
    /// <returns>A resource key in the form <c>WeatherCode_{Value}</c>.</returns>
    public static string ToResourceKey(this WeatherCode weatherCode)
    {
        return $"WeatherCode_{weatherCode}";
    }
}
