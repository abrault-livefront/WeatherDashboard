namespace WeatherDashboard.Infrastructure.Services.Search;

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Abstractions;
using Domain.Entities.Documents;
using Extensions;
using Indexer.Abstractions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides search functionality for location documents using Lucene.Net full-text search.
/// </summary>
/// <remarks>
///     This service searches across locality names, provinces, province codes, and postal codes.
///     It uses a per-field analyzer configuration where locality and province use standard analysis,
///     while province codes and postal codes use keyword analysis for exact matching.
/// </remarks>
public sealed class LocationSearchService : LuceneSearchService<LocationDocument>
{
    private readonly ILogger<LocationSearchService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocationSearchService" /> class.
    /// </summary>
    /// <param name="indexerService">The Lucene indexer service used to manage the search index.</param>
    /// <param name="logger">The logger instance for recording search operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger" /> is <c>null</c>.</exception>
    public LocationSearchService(ILuceneIndexerService<LocationDocument> indexerService,
                                 ILogger<LocationSearchService> logger)
        : base(indexerService, logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    protected override Query? CreateQuery(string query)
    {
        if ( string.IsNullOrWhiteSpace(query) || query.Length < 3 )
        {
            return null;
        }

        // Sanitize the query by removing wildcard characters and normalizing case
        query = query.Replace("*", string.Empty, StringComparison.OrdinalIgnoreCase)
                     .Replace("?", string.Empty, StringComparison.OrdinalIgnoreCase)
                     .Trim()
                     .ToLowerInvariant();

        string[] terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Phrase match for all but the last term, prefixed match for the last term
        BooleanQuery localCompositeQuery = [];

        if ( terms.Length == 1 )
        {
            localCompositeQuery.Add(new PrefixQuery(new Term(nameof(LocationDocument.Locality), terms[0])),
                Occur.MUST
            );
        }
        else
        {
            PhraseQuery phrase = [];

            foreach ( string term in terms )
            {
                phrase.Add(new Term(nameof(LocationDocument.Locality), term));
            }

            localCompositeQuery.Add(phrase, Occur.MUST);

            // Add the last term as a wildcard term for partial matching
            PrefixQuery lastPrefix = new(new Term(nameof(LocationDocument.Locality), terms[^1]));

            localCompositeQuery.Add(lastPrefix, Occur.MUST);
        }

        PrefixQuery postalCodeQuery = new(new Term(nameof(LocationDocument.PostalCodes), query));

        return new BooleanQuery
        {
            { localCompositeQuery, Occur.SHOULD },
            { postalCodeQuery, Occur.SHOULD },
        };
    }

    /// <inheritdoc />
    protected override LocationDocument? FromLuceneDocument(Document document)
    {
        string idAsString = document.GetField(nameof(LocationDocument.Id)).GetStringValue(CultureInfo.InvariantCulture);
        if ( !Guid.TryParseExact(idAsString, "D", out Guid id) )
        {
            _logger.LogFailedToParseDocumentId(idAsString);
            return null;
        }

        string locality = document.GetField(nameof(LocationDocument.Locality))
                                  .GetStringValue(CultureInfo.InvariantCulture);

        string province = document.GetField(nameof(LocationDocument.Province))
                                  .GetStringValue(CultureInfo.InvariantCulture);

        string provinceCode = document.GetField(nameof(LocationDocument.ProvinceCode))
                                      .GetStringValue(CultureInfo.InvariantCulture);

        ReadOnlyCollection<string> postalCodes = document.GetFields(nameof(LocationDocument.PostalCodes))
                                                         .Select(field => field.GetStringValue(CultureInfo.InvariantCulture))
                                                         .Where(value => !string.IsNullOrWhiteSpace(value))
                                                         .ToList()
                                                         .AsReadOnly();

        double latitude = document.GetField(nameof(LocationDocument.Latitude))
                                  .GetDoubleValue() ?? double.NaN;

        double longitude = document.GetField(nameof(LocationDocument.Longitude))
                                   .GetDoubleValue() ?? double.NaN;

        return new LocationDocument(
            id,
            locality,
            province,
            provinceCode,
            postalCodes,
            latitude,
            longitude);
    }
}
