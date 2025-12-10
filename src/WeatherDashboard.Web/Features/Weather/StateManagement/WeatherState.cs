namespace WeatherDashboard.Web.Features.Weather.StateManagement;

using System.Diagnostics.CodeAnalysis;
using Application.Contracts.Weather;
using Fluxor;
using JetBrains.Annotations;

/// <summary>
///     Represents the Fluxor state for weather forecast data.
/// </summary>
/// <remarks>
///     This state is managed by Fluxor and contains the current forecast and loading status.
///     Reducers update this state in response to weather-related actions.
/// </remarks>
[UsedImplicitly]
[FeatureState]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
public sealed record WeatherState
{
    /// <summary>
    ///     Gets or initializes the current weather forecast.
    ///     This is <see langword="null" /> when no forecast has been loaded or when loading fails.
    /// </summary>
    public ForecastCacheContract? CurrentForecast { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether a weather forecast is currently being loaded.
    /// </summary>
    public bool IsLoading { get; init; }
}
