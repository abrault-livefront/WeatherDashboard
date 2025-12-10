namespace WeatherDashboard.Infrastructure.Serialization.Weather;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Services.Weather.Responses;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(ForecastResponse))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal sealed partial class ForecastResponseJsonSerializerContext : JsonSerializerContext;
