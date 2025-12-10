namespace WeatherDashboard.Infrastructure.Extensions;

using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Domain.Entities.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Persistence;
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
    private static readonly DirectoryInfo IndexDirectory =
        new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WeatherDashboard",
            "Indexes"));

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
    ///             <description>Lucene directory factory as a singleton.</description>
    ///         </item>
    ///         <item>
    ///             <description>Location indexer service as a singleton.</description>
    ///         </item>
    ///         <item>
    ///             <description>Location search service as a singleton.</description>
    ///         </item>
    ///         <item>
    ///             <description>Location indexer background service as a hosted service.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ITimeProvider, SystemTimeProvider>();
        services.TryAddSingleton<IRateLimitTracker, WeatherApiRateLimitTracker>();

        services.AddHttpClient<WeatherApiClient>()
                .AddStandardResilienceHandler();

        services.TryAddSingleton<LuceneDirectoryFactory>();

        services.TryAddSingleton<ILuceneIndexerService<LocationDocument>>(provider =>
            new LocationIndexerService(IndexDirectory,
                provider.GetRequiredService<ILogger<LocationIndexerService>>(),
                provider.GetRequiredService<LuceneDirectoryFactory>()
            ));

        services.TryAddSingleton<ISearchService<LocationDocument>, LocationSearchService>();

        services.AddHostedService(provider =>
            new LocationIndexerBackgroundService(IndexDirectory,
                provider.GetRequiredService<ILuceneIndexerService<LocationDocument>>(),
                provider.GetRequiredService<ILogger<LocationIndexerBackgroundService>>()));


        services.TryAddScoped<IWeatherService, WeatherService>();

        return services;
    }
}
