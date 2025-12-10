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
///     Background service that indexes location documents from embedded seed data using Lucene.Net.
/// </summary>
/// <remarks>
///     This service runs on application startup and checks if the location index needs to be rebuilt
///     by comparing the hash of the embedded seed data with a stored hash file. If the data has changed
///     or no index exists, it deserializes the location documents and builds the search index.
/// </remarks>
public sealed class LocationIndexerBackgroundService : BackgroundService
{
    private const string ResourceName = "WeatherDashboard.Infrastructure.Data.SeedData.Locations.json";

    private readonly ILuceneIndexerService<LocationDocument> _indexerService;

    private readonly FileInfo _indexHashFileName;

    private readonly ILogger<LocationIndexerBackgroundService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocationIndexerBackgroundService" /> class.
    /// </summary>
    /// <param name="indexDirectory">The directory where the index files are to be stored.</param>
    /// <param name="indexerService">The Lucene indexer service used to build and manage the location index.</param>
    /// <param name="logger">The logger instance for recording indexing operations and errors.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="indexDirectory" />, <paramref name="indexerService" /> or
    ///     <paramref name="logger" /> is <c>null</c>.
    /// </exception>
    public LocationIndexerBackgroundService(DirectoryInfo indexDirectory,
                                            ILuceneIndexerService<LocationDocument> indexerService,
                                            ILogger<LocationIndexerBackgroundService> logger)
    {
        ArgumentNullException.ThrowIfNull(indexDirectory);

        _indexerService = indexerService ?? throw new ArgumentNullException(nameof(indexerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _indexHashFileName = new FileInfo(Path.Combine(indexDirectory.FullName,
            "Metadata",
            $"{_indexerService.IndexName}.version"));
    }

    /// <summary>
    ///     Executes the background indexing operation asynchronously.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to cancel the background operation.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method loads location data from an embedded JSON resource, computes its hash, and compares it
    ///     with the stored hash to determine if the index needs rebuilding. If rebuilding is required, it
    ///     deserializes the documents, indexes them, optimizes the index, and saves the new hash.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if ( !_indexHashFileName.Directory!.Exists )
        {
            Directory.CreateDirectory(_indexHashFileName.Directory.FullName);
        }

        if ( !_indexerService.IndexDirectory.Exists )
        {
            Directory.CreateDirectory(_indexerService.IndexDirectory.FullName);
        }

        Assembly assembly = Assembly.GetExecutingAssembly();

        Stream? stream = assembly.GetManifestResourceStream(ResourceName);
        if ( stream is null )
        {
            _logger.LogResourceCouldNotBeLocated(ResourceName, assembly.FullName);
            return;
        }

        byte[] data;
        await using ( stream.ConfigureAwait(false) )
        {
            data = new byte[stream.Length];
            await stream.ReadExactlyAsync(data, stoppingToken).ConfigureAwait(false);
        }

        string hash = Convert.ToHexString(SHA256.HashData(data));

        if ( _indexHashFileName.Exists )
        {
            string fileHash = await File.ReadAllTextAsync(_indexHashFileName.FullName, stoppingToken)
                                        .ConfigureAwait(false);

            if ( string.Equals(hash, fileHash, StringComparison.OrdinalIgnoreCase) )
            {
                return; // Index is up to date
            }
        }

        // Index needs to be rebuilt (we cannot assume that the document identifiers are the same)
        _indexerService.RemoveAll();

        List<LocationDocument>? locations = JsonSerializer.Deserialize(data,
            DocumentJsonSerializerContext.Default.ListLocationDocument);

        if ( locations is null )
        {
            return;
        }

        _indexerService.IndexMany(locations);
        _indexerService.Optimize();

        await File.WriteAllTextAsync(_indexHashFileName.FullName, hash, stoppingToken)
                  .ConfigureAwait(false);
    }
}
