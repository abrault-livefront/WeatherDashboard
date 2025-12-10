namespace WeatherDashboard.Web.Features.Weather;

using System.Collections.Frozen;
using Domain.Entities.Weather.Enums;

/// <summary>
///     Maps weather conditions and wind data to corresponding weather icon class names.
/// </summary>
internal static class WeatherIconMapper
{
    /// <summary>
    ///     A frozen dictionary mapping weather codes to their corresponding day and night icon class names.
    /// </summary>
    private static readonly FrozenDictionary<WeatherCode, (string Day, string Night)> IconMap =
        new Dictionary<WeatherCode, (string Day, string Night)>
        {
            { WeatherCode.ClearSky, ( "wi-day-sunny", "wi-night-clear" ) },
            { WeatherCode.MainlyClear, ( "wi-day-sunny-overcast", "wi-night-alt-partly-cloudy" ) },
            { WeatherCode.PartlyCloudy, ( "wi-day-cloudy", "wi-night-alt-cloudy" ) },
            { WeatherCode.Overcast, ( "wi-cloudy", "wi-cloudy" ) },
            { WeatherCode.Fog, ( "wi-day-fog", "wi-night-fog" ) },
            { WeatherCode.DepositingRimeFog, ( "wi-day-fog", "wi-night-fog" ) },
            { WeatherCode.DrizzleLight, ( "wi-day-sprinkle", "wi-night-alt-sprinkle" ) },
            { WeatherCode.DrizzleModerate, ( "wi-day-sprinkle", "wi-night-alt-sprinkle" ) },
            { WeatherCode.DrizzleDense, ( "wi-day-showers", "wi-night-alt-showers" ) },
            { WeatherCode.FreezingDrizzleLight, ( "wi-day-sleet", "wi-night-alt-sleet" ) },
            { WeatherCode.FreezingDrizzleDense, ( "wi-day-sleet", "wi-night-alt-sleet" ) },
            { WeatherCode.RainSlight, ( "wi-day-rain", "wi-night-alt-rain" ) },
            { WeatherCode.RainModerate, ( "wi-day-rain", "wi-night-alt-rain" ) },
            { WeatherCode.RainHeavy, ( "wi-day-rain-wind", "wi-night-alt-rain-wind" ) },
            { WeatherCode.FreezingRainLight, ( "wi-day-rain-mix", "wi-night-alt-rain-mix" ) },
            { WeatherCode.FreezingRainHeavy, ( "wi-day-rain-mix", "wi-night-alt-rain-mix" ) },
            { WeatherCode.SnowFallSlight, ( WeatherDaySnow, WeatherNightSnow ) },
            { WeatherCode.SnowFallModerate, ( WeatherDaySnow, WeatherNightSnow ) },
            { WeatherCode.SnowFallHeavy, ( "wi-day-snow-wind", "wi-night-alt-snow-wind" ) },
            { WeatherCode.SnowGrains, ( WeatherDaySnow, WeatherNightSnow ) },
            { WeatherCode.RainShowersSlight, ( "wi-day-showers", "wi-night-alt-showers" ) },
            { WeatherCode.RainShowersModerate, ( "wi-day-showers", "wi-night-alt-showers" ) },
            { WeatherCode.RainShowersViolent, ( "wi-day-rain-wind", "wi-night-alt-rain-wind" ) },
            { WeatherCode.SnowShowersSlight, ( WeatherDaySnow, WeatherNightSnow ) },
            { WeatherCode.SnowShowersHeavy, ( "wi-day-snow-wind", "wi-night-alt-snow-wind" ) },
            { WeatherCode.ThunderstormSlightOrModerate, ( "wi-day-thunderstorm", "wi-night-alt-thunderstorm" ) },
            { WeatherCode.ThunderstormWithHail, ( "wi-day-storm-showers", "wi-night-alt-storm-showers" ) },
            { WeatherCode.ThunderstormWithSevereHail, ( "wi-day-storm-showers", "wi-night-alt-storm-showers" ) },
        }.ToFrozenDictionary();

    private const string WeatherDaySnow = "wi-day-snow";

    private const string WeatherNightSnow = "wi-night-alt-snow";

    /// <summary>
    ///     Gets the Beaufort wind scale icon class name based on wind speed.
    /// </summary>
    /// <param name="windSpeed">The wind speed in mph.</param>
    /// <returns>A weather icon class name representing the Beaufort scale (0-12).</returns>
    public static string GetBeaufortWindScaleIcon(double windSpeed)
    {
        return windSpeed switch
        {
            < 1 => "wi-wind-beaufort-0",
            >= 1 and < 4 => "wi-wind-beaufort-1",
            >= 4 and < 8 => "wi-wind-beaufort-2",
            >= 8 and < 13 => "wi-wind-beaufort-3",
            >= 13 and < 19 => "wi-wind-beaufort-4",
            >= 19 and < 25 => "wi-wind-beaufort-5",
            >= 25 and < 32 => "wi-wind-beaufort-6",
            >= 32 and < 39 => "wi-wind-beaufort-7",
            >= 39 and < 47 => "wi-wind-beaufort-8",
            >= 47 and < 55 => "wi-wind-beaufort-9",
            >= 55 and < 64 => "wi-wind-beaufort-10",
            >= 64 and < 73 => "wi-wind-beaufort-11",
            _ => "wi-wind-beaufort-12",
        };
    }

    /// <summary>
    ///     Gets the weather icon class name for a given weather code.
    /// </summary>
    /// <param name="code">The weather code to map to an icon.</param>
    /// <param name="isNight">Indicates whether to return the night variant of the icon. Default is false.</param>
    /// <returns>A weather icon class name, or "wi-na" if the code is null or not found.</returns>
    public static string GetWeatherCodeIcon(WeatherCode? code, bool isNight = false)
    {
        if ( code is null || !IconMap.TryGetValue((WeatherCode)code, out (string Day, string Night) icons) )
        {
            return "wi-na";
        }

        return isNight ? icons.Night : icons.Day;
    }

    /// <summary>
    ///     Gets the wind direction icon class name based on a cardinal direction.
    ///     The icon represents the direction the wind is blowing towards.
    /// </summary>
    /// <param name="cardinalDirection">The cardinal direction abbreviation (e.g., "N", "NE", "S", "SW").</param>
    /// <returns>A weather icon class name representing wind direction, or "wi-na" if the input is invalid.</returns>
    public static string GetWindDirectionIcon(string? cardinalDirection)
    {
        if ( string.IsNullOrWhiteSpace(cardinalDirection) )
        {
            return "wi-na";
        }

        #pragma warning disable CA1308
        string direction = cardinalDirection.Trim().ToLowerInvariant();
        #pragma warning restore CA1308

        Dictionary<string, string> oppositeDirections = new(StringComparer.OrdinalIgnoreCase)
        {
            { "n", "s" },
            { "nne", "ssw" },
            { "ne", "sw" },
            { "ene", "wsw" },
            { "e", "w" },
            { "ese", "wnw" },
            { "se", "nw" },
            { "sse", "nnw" },
            { "s", "n" },
            { "ssw", "nne" },
            { "sw", "ne" },
            { "wsw", "ene" },
            { "w", "e" },
            { "wnw", "ese" },
            { "nw", "se" },
            { "nnw", "sse" },
        };

        return oppositeDirections.TryGetValue(direction, out string? oppositeDirection)
                   ? $"wi-towards-{oppositeDirection}"
                   : $"wi-from-{direction}";
    }
}
