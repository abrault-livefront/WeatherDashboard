namespace WeatherDashboard.Application.UnitTests.Extensions;

using AwesomeAssertions;
using Common.Extensions;
using Domain.Entities.Weather.Enums;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "Weather")]
[Trait("Speed", "Fast")]
public sealed class WeatherCodeExtensionsTests
{
    [Theory]
    [InlineData(WeatherCode.ClearSky, "WeatherCode_ClearSky")]
    [InlineData(WeatherCode.MainlyClear, "WeatherCode_MainlyClear")]
    [InlineData(WeatherCode.PartlyCloudy, "WeatherCode_PartlyCloudy")]
    [InlineData(WeatherCode.Overcast, "WeatherCode_Overcast")]
    [InlineData(WeatherCode.Fog, "WeatherCode_Fog")]
    [InlineData(WeatherCode.DepositingRimeFog, "WeatherCode_DepositingRimeFog")]
    [InlineData(WeatherCode.DrizzleLight, "WeatherCode_DrizzleLight")]
    [InlineData(WeatherCode.DrizzleModerate, "WeatherCode_DrizzleModerate")]
    [InlineData(WeatherCode.DrizzleDense, "WeatherCode_DrizzleDense")]
    [InlineData(WeatherCode.FreezingDrizzleLight, "WeatherCode_FreezingDrizzleLight")]
    [InlineData(WeatherCode.FreezingDrizzleDense, "WeatherCode_FreezingDrizzleDense")]
    [InlineData(WeatherCode.RainSlight, "WeatherCode_RainSlight")]
    [InlineData(WeatherCode.RainModerate, "WeatherCode_RainModerate")]
    [InlineData(WeatherCode.RainHeavy, "WeatherCode_RainHeavy")]
    [InlineData(WeatherCode.FreezingRainLight, "WeatherCode_FreezingRainLight")]
    [InlineData(WeatherCode.FreezingRainHeavy, "WeatherCode_FreezingRainHeavy")]
    [InlineData(WeatherCode.SnowFallSlight, "WeatherCode_SnowFallSlight")]
    [InlineData(WeatherCode.SnowFallModerate, "WeatherCode_SnowFallModerate")]
    [InlineData(WeatherCode.SnowFallHeavy, "WeatherCode_SnowFallHeavy")]
    [InlineData(WeatherCode.SnowGrains, "WeatherCode_SnowGrains")]
    [InlineData(WeatherCode.RainShowersSlight, "WeatherCode_RainShowersSlight")]
    [InlineData(WeatherCode.RainShowersModerate, "WeatherCode_RainShowersModerate")]
    [InlineData(WeatherCode.RainShowersViolent, "WeatherCode_RainShowersViolent")]
    [InlineData(WeatherCode.SnowShowersSlight, "WeatherCode_SnowShowersSlight")]
    [InlineData(WeatherCode.SnowShowersHeavy, "WeatherCode_SnowShowersHeavy")]
    [InlineData(WeatherCode.ThunderstormSlightOrModerate, "WeatherCode_ThunderstormSlightOrModerate")]
    [InlineData(WeatherCode.ThunderstormWithHail, "WeatherCode_ThunderstormWithHail")]
    [InlineData(WeatherCode.ThunderstormWithSevereHail, "WeatherCode_ThunderstormWithSevereHail")]
    public void ToResourceKey_WithSpecificWeatherCode_ReturnsExpectedString(WeatherCode weatherCode, string expectedResourceKey)
    {
        string resourceKey = weatherCode.ToResourceKey();

        resourceKey.Should().Be(expectedResourceKey);
    }
}
