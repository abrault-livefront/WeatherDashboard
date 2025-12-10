namespace WeatherDashboard.Web.Features.Weather.StateManagement.Effects;

using System.Diagnostics.CodeAnalysis;
using Actions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Contracts.Weather;
using Domain.Entities.Weather;
using Fluxor;
using J2N;
using ZiggyCreatures.Caching.Fusion;

/// <summary>
///     Fluxor effect that handles fetching weather forecast data with caching.
/// </summary>
/// <remarks>
///     This effect responds to <see cref="FetchWeatherAction" /> by retrieving weather data from the
///     weather service, caching the result for 15 minutes, and dispatching either
///     <see cref="FetchWeatherSuccessAction" /> or <see cref="FetchWeatherFailureAction" />.
/// </remarks>
[SuppressMessage("Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Effect instantiated by Fluxor")]
internal sealed class FetchWeatherEffect : Effect<FetchWeatherAction>
{
    private readonly IFusionCache _cache;

    private readonly IWeatherService _weatherService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FetchWeatherEffect" /> class.
    /// </summary>
    /// <param name="cache">The fusion cache used to cache weather forecast data.</param>
    /// <param name="weatherService">The weather service used to fetch forecast data.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="cache" /> or <paramref name="weatherService" /> is <see langword="null" />.
    /// </exception>
    public FetchWeatherEffect(IFusionCache cache, IWeatherService weatherService)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
    }

    /// <summary>
    ///     Handles the <see cref="FetchWeatherAction" /> by fetching weather forecast data
    ///     and dispatching the appropriate success or failure action.
    /// </summary>
    /// <param name="action">The action containing the latitude and longitude coordinates.</param>
    /// <param name="dispatcher">The Fluxor dispatcher used to dispatch result actions.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="action" /> or <paramref name="dispatcher" /> is <see langword="null" />.
    /// </exception>
    public override Task HandleAsync(FetchWeatherAction action, IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(dispatcher);

        if ( !action.Latitude.IsInfinity() && !action.Longitude.IsInfinity() )
        {
            return HandleInternalAsync(action, dispatcher);
        }

        dispatcher.Dispatch(new FetchWeatherFailureAction());
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Internal handler that fetches weather data from cache or service, then dispatches the result.
    /// </summary>
    /// <param name="action">The action containing the coordinates.</param>
    /// <param name="dispatcher">The Fluxor dispatcher.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleInternalAsync(FetchWeatherAction action, IDispatcher dispatcher)
    {
        string cacheKey = HashUtility.HashString($"weather-{action.Latitude}-{action.Longitude}");

        ForecastCacheContract? result = await _cache.GetOrSetAsync(
                                            cacheKey,
                                            async token => await RequestWeatherAsync(
                                                                   action.Latitude,
                                                                   action.Longitude,
                                                                   token
                                                               )
                                                              .ConfigureAwait(false),
                                            options => options.SetDuration(TimeSpan.FromMinutes(15))
                                        ).ConfigureAwait(false);

        if ( result is null )
        {
            dispatcher.Dispatch(new FetchWeatherFailureAction());
        }
        else
        {
            dispatcher.Dispatch(new FetchWeatherSuccessAction(result));
        }
    }

    /// <summary>
    ///     Requests weather data from the weather service and converts it to a cache contract.
    /// </summary>
    /// <param name="latitude">The latitude coordinate.</param>
    /// <param name="longitude">The longitude coordinate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A <see cref="ForecastCacheContract" /> if the request succeeds, or <see langword="null" /> if it fails.
    /// </returns>
    private async Task<ForecastCacheContract?> RequestWeatherAsync(double latitude,
                                                                   double longitude,
                                                                   CancellationToken cancellationToken)
    {
        Forecast? result = await _weatherService.RequestAsync(latitude, longitude, cancellationToken)
                                                .ConfigureAwait(false);

        return result is not null
                   ? ForecastCacheContractMapper.ToContract(result)
                   : null;
    }
}
