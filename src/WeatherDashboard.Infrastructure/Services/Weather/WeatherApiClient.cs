namespace WeatherDashboard.Infrastructure.Services.Weather;

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Json;
using Application.Common.Interfaces;
using Responses;
using Serialization.Weather;

[SuppressMessage("Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "This class is registered in the DI container.")]
internal sealed class WeatherApiClient
{
    private static readonly Uri BaseUri = new("https://api.open-meteo.com/v1/forecast");

    private static readonly FrozenSet<string> DailyParameters = new[]
    {
        "apparent_temperature_max",
        "dew_point_2m_max",
        "relative_humidity_2m_min",
        "sunrise",
        "sunset",
        "surface_pressure_min",
        "uv_index_max",
        "visibility_mean",
        "weather_code",
        "wind_direction_10m_dominant",
        "wind_gusts_10m_min",
        "wind_speed_10m_min",
    }.ToFrozenSet();

    private static readonly string DailyParametersString = string.Join(',', DailyParameters);

    private readonly HttpClient _httpClient;

    private readonly IRateLimitTracker _rateLimitTracker;

    public WeatherApiClient(HttpClient httpClient, IRateLimitTracker rateLimitTracker)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _rateLimitTracker = rateLimitTracker ?? throw new ArgumentNullException(nameof(rateLimitTracker));
    }

    public async Task<WeatherApiResponse?> RequestAsync(double latitude,
                                                        double longitude,
                                                        CancellationToken cancellationToken = default)
    {
        if ( !await _rateLimitTracker.CanMakeRequestAsync(cancellationToken).ConfigureAwait(false) )
        {
            throw new InvalidOperationException("Rate limit exceeded. Please try again later.");
        }

        if ( cancellationToken.IsCancellationRequested )
        {
            return null;
        }

        Dictionary<string, string> queryParameters = new(7, StringComparer.OrdinalIgnoreCase)
        {
            { "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
            { "longitude", longitude.ToString(CultureInfo.InvariantCulture) },
            { "daily", DailyParametersString },
            { "wind_speed_unit", "mph" },
            { "temperature_unit", "fahrenheit" },
            { "precipitation_unit", "inch" },
            { "timezone", "auto" },
        };

        string queryString = string.Join('&',
            queryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        Uri requestUri = new(BaseUri, $"?{queryString}");

        using HttpResponseMessage response = await _httpClient.GetAsync(requestUri,
                                                                   HttpCompletionOption.ResponseHeadersRead,
                                                                   cancellationToken
                                                               )
                                                              .ConfigureAwait(false);

        await _rateLimitTracker.RecordRequestAsync(cancellationToken).ConfigureAwait(false);

        if ( !response.IsSuccessStatusCode )
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync(WeatherApiResponseJsonSerializerContext.Default.WeatherApiResponse, cancellationToken)
                             .ConfigureAwait(false);
    }
}
