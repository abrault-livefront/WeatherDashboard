namespace WeatherDashboard.Application.Common.Interfaces;

using Domain.Entities.Weather;

/// <summary>
///     Defines a service for retrieving weather forecast data.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    ///     Requests weather forecast data for the specified geographic coordinates.
    /// </summary>
    /// <param name="latitude">The latitude coordinate of the location.</param>
    /// <param name="longitude">The longitude coordinate of the location.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the weather forecast,
    ///     or null if the forecast could not be retrieved.
    /// </returns>
    Task<Forecast?> RequestAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
