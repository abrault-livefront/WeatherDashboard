namespace WeatherDashboard.Infrastructure.Services.BackgroundServices;

using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Domain.Entities.Documents;
using Domain.Serialization.Json;
using Extensions;
using Indexer.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     Background service that indexes location documents from embedded seed data into Elasticsearch.
/// </summary>
/// <remarks>
///     This service runs on application startup and checks if the location index needs to be rebuilt
///     by comparing the hash of the embedded seed data with a hash recorded in Elasticsearch from the
///     previous successful seed. If the data has changed or no index exists, it deserializes the
///     location documents and rebuilds the search index.
/// </remarks>
public sealed class LocationIndexerBackgroundService : BackgroundService
{
    private const string s_resourceName = "WeatherDashboard.Infrastructure.Data.SeedData.Locations.json";

    private readonly IElasticsearchIndexerService<LocationDocument> _indexerService;

    private readonly ILogger<LocationIndexerBackgroundService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocationIndexerBackgroundService" /> class.
    /// </summary>
    /// <param name="indexerService">The Elasticsearch indexer service used to build and manage the location index.</param>
    /// <param name="logger">The logger instance for recording indexing operations and errors.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="indexerService" /> or <paramref name="logger" /> is <c>null</c>.
    /// </exception>
    public LocationIndexerBackgroundService(IElasticsearchIndexerService<LocationDocument> indexerService,
                                            ILogger<LocationIndexerBackgroundService> logger)
    {
        _indexerService = indexerService ?? throw new ArgumentNullException(nameof(indexerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Executes the background indexing operation asynchronously.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to cancel the background operation.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method loads location data from an embedded JSON resource, computes its hash, and compares it
    ///     with the hash recorded from the previous seed to determine if the index needs rebuilding. If rebuilding
    ///     is required, it deserializes the documents, indexes them, and records the new hash.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _indexerService.EnsureIndexExistsAsync(stoppingToken).ConfigureAwait(false);

        Assembly assembly = Assembly.GetExecutingAssembly();

        Stream? stream = assembly.GetManifestResourceStream(s_resourceName);
        if ( stream is null )
        {
            _logger.LogResourceCouldNotBeLocated(s_resourceName, assembly.FullName);
            return;
        }

        byte[] data;
        await using ( stream.ConfigureAwait(false) )
        {
            data = new byte[stream.Length];
            await stream.ReadExactlyAsync(data, stoppingToken).ConfigureAwait(false);
        }

        string hash = Convert.ToHexString(SHA256.HashData(data));

        string? existingHash = await _indexerService.GetSeedHashAsync(stoppingToken).ConfigureAwait(false);

        if ( string.Equals(hash, existingHash, StringComparison.OrdinalIgnoreCase) )
        {
            return; // Index is up to date
        }

        // Index needs to be rebuilt (we cannot assume that the document identifiers are the same)
        await _indexerService.RemoveAllAsync(stoppingToken).ConfigureAwait(false);

        List<LocationDocument>? locations = JsonSerializer.Deserialize(data,
            DocumentJsonSerializerContext.Default.ListLocationDocument);

        if ( locations is null )
        {
            return;
        }

        await _indexerService.IndexManyAsync(locations, stoppingToken).ConfigureAwait(false);

        await _indexerService.SaveSeedHashAsync(hash, stoppingToken).ConfigureAwait(false);
    }
}
