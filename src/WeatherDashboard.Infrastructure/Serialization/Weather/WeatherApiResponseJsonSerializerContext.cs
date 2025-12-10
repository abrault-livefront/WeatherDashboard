namespace WeatherDashboard.Infrastructure.Serialization.Weather;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Services.Weather.Responses;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(WeatherApiResponse))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class WeatherApiResponseJsonSerializerContext : JsonSerializerContext;
