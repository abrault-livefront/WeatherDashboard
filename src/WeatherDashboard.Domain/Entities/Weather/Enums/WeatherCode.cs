namespace WeatherDashboard.Domain.Entities.Weather.Enums;

/// <summary>
///     WMO weather condition codes with user-friendly descriptions for UI display.
/// </summary>
public enum WeatherCode
{
    /// <summary>
    ///     Clear sky.
    /// </summary>
    ClearSky = 0,

    /// <summary>
    ///     Mostly clear.
    /// </summary>
    MainlyClear = 1,

    /// <summary>
    ///     Partly cloudy.
    /// </summary>
    PartlyCloudy = 2,

    /// <summary>
    ///     Overcast.
    /// </summary>
    Overcast = 3,

    /// <summary>
    ///     Fog.
    /// </summary>
    Fog = 45,

    /// <summary>
    ///     Freezing fog.
    /// </summary>
    DepositingRimeFog = 48,

    /// <summary>
    ///     Light drizzle.
    /// </summary>
    DrizzleLight = 51,

    /// <summary>
    ///     Moderate drizzle.
    /// </summary>
    DrizzleModerate = 53,

    /// <summary>
    ///     Heavy drizzle.
    /// </summary>
    DrizzleDense = 55,

    /// <summary>
    ///     Light freezing drizzle.
    /// </summary>
    FreezingDrizzleLight = 56,

    /// <summary>
    ///     Heavy freezing drizzle.
    /// </summary>
    FreezingDrizzleDense = 57,

    /// <summary>
    ///     Light rain.
    /// </summary>
    RainSlight = 61,

    /// <summary>
    ///     Moderate rain.
    /// </summary>
    RainModerate = 63,

    /// <summary>
    ///     Heavy rain.
    /// </summary>
    RainHeavy = 65,

    /// <summary>
    ///     Light freezing rain.
    /// </summary>
    FreezingRainLight = 66,

    /// <summary>
    ///     Heavy freezing rain.
    /// </summary>
    FreezingRainHeavy = 67,

    /// <summary>
    ///     Light snow.
    /// </summary>
    SnowFallSlight = 71,

    /// <summary>
    ///     Moderate snow.
    /// </summary>
    SnowFallModerate = 73,

    /// <summary>
    ///     Heavy snow.
    /// </summary>
    SnowFallHeavy = 75,

    /// <summary>
    ///     Snow grains.
    /// </summary>
    SnowGrains = 77,

    /// <summary>
    ///     Light rain showers.
    /// </summary>
    RainShowersSlight = 80,

    /// <summary>
    ///     Moderate rain showers.
    /// </summary>
    RainShowersModerate = 81,

    /// <summary>
    ///     Heavy rain showers.
    /// </summary>
    RainShowersViolent = 82,

    /// <summary>
    ///     Light snow showers.
    /// </summary>
    SnowShowersSlight = 85,

    /// <summary>
    ///     Heavy snow showers.
    /// </summary>
    SnowShowersHeavy = 86,

    /// <summary>
    ///     Thunderstorm.
    /// </summary>
    ThunderstormSlightOrModerate = 95,

    /// <summary>
    ///     Thunderstorm with hail.
    /// </summary>
    ThunderstormWithHail = 96,

    /// <summary>
    ///     Severe thunderstorm with large hail.
    /// </summary>
    ThunderstormWithSevereHail = 99,
}
