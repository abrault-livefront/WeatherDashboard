namespace WeatherDashboard.Application.Contracts.Weather;

using Domain.Entities.Weather;
using Riok.Mapperly.Abstractions;

/// <summary>
///     Provides mapping functionality between <see cref="Forecast" /> entities and <see cref="ForecastCacheContract" />
///     data transfer objects.
/// </summary>
[Mapper]
public static partial class ForecastCacheContractMapper
{
    /// <summary>
    ///     Converts a <see cref="Forecast" /> entity to a <see cref="ForecastCacheContract" />.
    /// </summary>
    /// <param name="forecast">The forecast entity to convert.</param>
    /// <returns>A cache contract containing the forecast data.</returns>
    public static partial ForecastCacheContract ToContract(Forecast forecast);
}
