namespace WeatherDashboard.Web.Features.Weather;

using System.Globalization;
using TimeZoneNames;
using UnitsNet;
using UnitsNet.Units;

/// <summary>
///     Provides utility methods for weather-related calculations and formatting.
/// </summary>
internal static class WeatherHelpers
{
    /// <summary>
    ///     Formats a date and time with a time zone abbreviation suffix.
    /// </summary>
    /// <param name="dateTime">The date and time to format. If null, an empty string is returned.</param>
    /// <param name="timeZoneInfo">The time zone to use for formatting the abbreviation. If null, UTC is used.</param>
    /// <returns>
    ///     A formatted time string with the time zone abbreviation appended as a suffix (e.g., "3:45 PM EST").
    ///     Returns an empty string if <paramref name="dateTime" /> is null.
    ///     If no time zone abbreviation is available, returns only the formatted time without a suffix.
    /// </returns>
    public static string FormatTimeWithTimeZoneSuffix(DateTimeOffset? dateTime, TimeZoneInfo? timeZoneInfo)
    {
        if ( dateTime is null )
        {
            return string.Empty;
        }

        timeZoneInfo ??= TimeZoneInfo.Utc;

        string formatted = dateTime.Value.ToString("t", CultureInfo.CurrentCulture);
        TimeZoneValues suffix = TZNames.GetAbbreviationsForTimeZone(timeZoneInfo.Id,
            CultureInfo.CurrentCulture.Name);

        return string.IsNullOrWhiteSpace(suffix.Standard)
                   ? formatted
                   : string.Concat(formatted, " ", suffix.Standard);
    }

    /// <summary>
    ///     Formats wind speed information, including gusts if applicable.
    /// </summary>
    /// <param name="speed">The sustained wind speed in mph.</param>
    /// <param name="gusts">The wind gust speed in mph.</param>
    /// <param name="isMetric">Indicates whether to display wind speed in metric units (km/h) or imperial units (mph).</param>
    /// <returns>A formatted string displaying wind speed, with gust range if gusts exceed sustained speed.</returns>
    public static string FormatWindSpeed(double speed, double gusts, bool isMetric)
    {
        Speed speedValue = Speed.FromMilesPerHour(speed);
        Speed gustsValue = Speed.FromMilesPerHour(gusts);

        if ( gusts > speed )
        {
            return string.Format(CultureInfo.CurrentCulture,
                "{0:F1} - {1}",
                speedValue.ToUnit(isMetric ? SpeedUnit.KilometerPerHour : SpeedUnit.MilePerHour)
                          .Value,
                gustsValue.ToUnit(isMetric ? SpeedUnit.KilometerPerHour : SpeedUnit.MilePerHour)
                          .ToString(CultureInfo.CurrentCulture));
        }

        return speedValue.ToUnit(isMetric ? SpeedUnit.KilometerPerHour : SpeedUnit.MilePerHour)
                         .ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///     Converts wind direction from degrees to a compass direction abbreviation.
    /// </summary>
    /// <param name="degrees">The wind direction in degrees (0-360).</param>
    /// <returns>A compass direction abbreviation (e.g., "N", "NE", "S", "SW").</returns>
    public static string GetWindDirection(double degrees)
    {
        string[] directions =
        [
            "N",
            "NNE",
            "NE",
            "ENE",
            "E",
            "ESE",
            "SE",
            "SSE",
            "S",
            "SSW",
            "SW",
            "WSW",
            "W",
            "WNW",
            "NW",
            "NNW",
        ];

        int index = (int)Math.Round(degrees / 22.5) % 16;
        return directions[index];
    }

    /// <summary>
    ///     Determines whether it is currently nighttime for the specified time zone,
    ///     based on provided sunrise and sunset times.
    /// </summary>
    /// <param name="sunrise">The sunrise time as a <see cref="DateTimeOffset" /> in the forecast's local time.</param>
    /// <param name="sunset">The sunset time as a <see cref="DateTimeOffset" /> in the forecast's local time.</param>
    /// <param name="timeZone">The <see cref="TimeZoneInfo" /> representing the forecast's time zone.</param>
    /// <returns>
    ///     <see langword="true" /> if the current time in the given time zone is before <paramref name="sunrise" />
    ///     or at/after <paramref name="sunset" />; otherwise, <see langword="false" />.
    ///     Returns <see langword="false" /> if either <paramref name="sunrise" /> or <paramref name="sunset" /> is
    ///     <see langword="null" />.
    /// </returns>
    public static bool IsNightTime(DateTimeOffset? sunrise, DateTimeOffset? sunset, TimeZoneInfo timeZone)
    {
        if ( sunrise is null || sunset is null )
        {
            return false;
        }

        DateTimeOffset now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        return now < sunrise.Value || now >= sunset.Value;
    }
}
