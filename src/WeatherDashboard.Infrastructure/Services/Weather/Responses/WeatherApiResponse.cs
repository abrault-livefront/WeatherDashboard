namespace WeatherDashboard.Infrastructure.Services.Weather.Responses;

using System.Text.Json.Serialization;
using Application.Common.Serialization.Json;

internal sealed class WeatherApiResponse
{
    [JsonPropertyName("daily")]
    public required ForecastResponse Items { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    [JsonConverter(typeof(JsonStringTimeZoneInfoConverter))]
    public TimeZoneInfo TimeZone { get; set; } = null!;
}
