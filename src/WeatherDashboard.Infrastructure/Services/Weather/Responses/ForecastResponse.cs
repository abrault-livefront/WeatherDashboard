namespace WeatherDashboard.Infrastructure.Services.Weather.Responses;

using System.Text.Json.Serialization;
using Domain.Entities.Weather.Enums;

internal sealed class ForecastResponse
{
    [JsonPropertyName("dew_point_2m_max")]
    public double[] DewPoints { get; set; } = [];

    [JsonPropertyName("relative_humidity_2m_min")]
    public int[] Humidity { get; set; } = [];

    [JsonPropertyName("sunrise")]
    public DateTime[] SunriseTimes { get; set; } = [];

    [JsonPropertyName("sunset")]
    public DateTime[] SunsetTimes { get; set; } = [];

    [JsonPropertyName("surface_pressure_min")]
    public double[] SurfacePressures { get; set; } = [];

    [JsonPropertyName("apparent_temperature_max")]
    public double[] Temperatures { get; set; } = [];

    [JsonPropertyName("time")]
    public DateOnly[] Times { get; set; } = [];

    [JsonPropertyName("uv_index_max")]
    public double[] UvIndexes { get; set; } = [];

    [JsonPropertyName("visibility_mean")]
    public double[] Visibilities { get; set; } = [];

    [JsonPropertyName("weather_code")]
    public WeatherCode[] WeatherCodes { get; set; } = [];

    [JsonPropertyName("wind_direction_10m_dominant")]
    public int[] WindDirections { get; set; } = [];

    [JsonPropertyName("wind_gusts_10m_min")]
    public double[] WindGusts { get; set; } = [];

    [JsonPropertyName("wind_speed_10m_min")]
    public double[] WindSpeeds { get; set; } = [];
}
