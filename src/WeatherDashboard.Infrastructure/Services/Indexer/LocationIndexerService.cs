namespace WeatherDashboard.Infrastructure.Services.Indexer;

using Abstractions;
using Domain.Entities.Documents;
using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;
using Persistence;

/// <summary>
///     Provides an implementation of an indexer service using Apache Lucene.NET for indexing location documents.
/// </summary>
public sealed class LocationIndexerService : LuceneIndexerService<LocationDocument>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LocationIndexerService" /> class.
    /// </summary>
    /// <param name="indexDirectory">The directory where the index files are to be stored.</param>
    /// <param name="logger">The logger instance used for logging operations within the indexer service.</param>
    /// <param name="directoryFactory">The factory for creating and managing shared Lucene directory instances.</param>
    public LocationIndexerService(DirectoryInfo indexDirectory,
                                  ILogger<LocationIndexerService> logger,
                                  LuceneDirectoryFactory directoryFactory)
        : base("Locations", indexDirectory, logger, directoryFactory)
    {
    }

    /// <inheritdoc />
    protected override Document ToLuceneDocument(LocationDocument document)
    {
        Document doc =
        [
            new StringField(nameof(LocationDocument.Id), document.Id.ToString(), Field.Store.YES),
            new TextField(nameof(LocationDocument.Locality), document.Locality, Field.Store.YES),
            new TextField(nameof(LocationDocument.Province), document.Province, Field.Store.YES),
            new StoredField(nameof(LocationDocument.ProvinceCode), document.ProvinceCode),
            new StoredField(nameof(LocationDocument.Latitude), document.Latitude),
            new StoredField(nameof(LocationDocument.Longitude), document.Longitude),
        ];

        foreach ( string postalCode in document.PostalCodes )
        {
            doc.Add(new StringField(nameof(LocationDocument.PostalCodes), postalCode, Field.Store.YES));
        }

        return doc;
    }
}
