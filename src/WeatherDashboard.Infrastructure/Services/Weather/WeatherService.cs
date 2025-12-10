namespace WeatherDashboard.Infrastructure.Services.Weather;

using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Domain.Entities.Weather;
using Responses;

[SuppressMessage("Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "This class is registered in the DI container.")]
internal sealed class WeatherService : IWeatherService
{
    private readonly WeatherApiClient _client;

    public WeatherService(WeatherApiClient client)
    {
        _client = client;
    }

    public async Task<Forecast?> RequestAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        WeatherApiResponse? result = await _client.RequestAsync(latitude, longitude, cancellationToken)
                                                  .ConfigureAwait(false);

        return result is not null ? WeatherMapper.Map(result) : null;
    }
}
