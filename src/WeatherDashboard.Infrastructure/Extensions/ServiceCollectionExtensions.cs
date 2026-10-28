namespace WeatherDashboard.Infrastructure.Extensions;

using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Domain.Entities.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Providers;
using Services.BackgroundServices;
using Services.Indexer;
using Services.Indexer.Abstractions;
using Services.Search;
using Services.Weather;

/// <summary>
///     Provides extension methods for configuring infrastructure services in the dependency injection container.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds infrastructure services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     This method registers the following services:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Elasticsearch-backed location indexer service as a singleton.</description>
    ///         </item>
    ///         <item>
    ///             <description>Elasticsearch-backed location search service as a singleton.</description>
    ///         </item>
    ///         <item>
    ///             <description>Location indexer background service as a hosted service.</description>
    ///         </item>
    ///     </list>
    ///     Registration relies on an <see cref="Elastic.Clients.Elasticsearch.ElasticsearchClient" /> already being
    ///     registered in <paramref name="services" /> (e.g. via Aspire's <c>AddElasticsearchClient</c>) before this
    ///     method is called.
    /// </remarks>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ITimeProvider, SystemTimeProvider>();
        services.TryAddSingleton<IRateLimitTracker, WeatherApiRateLimitTracker>();

        services.AddHttpClient<WeatherApiClient>()
                .AddStandardResilienceHandler();

        services.TryAddSingleton<IElasticsearchIndexerService<LocationDocument>, ElasticsearchLocationIndexerService>();
        services.TryAddSingleton<ISearchService<LocationDocument>, ElasticsearchLocationSearchService>();

        services.AddHostedService<LocationIndexerBackgroundService>();

        services.TryAddScoped<IWeatherService, WeatherService>();

        return services;
    }
}
