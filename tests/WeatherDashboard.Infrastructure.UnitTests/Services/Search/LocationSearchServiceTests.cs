namespace WeatherDashboard.Infrastructure.UnitTests.Services.Search;

using AutoFixture;
using AwesomeAssertions;
using Domain.Common;
using Domain.Entities.Documents;
using Infrastructure.Services.Indexer.Abstractions;
using Infrastructure.Services.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using NSubstitute;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Fast")]
public sealed class LocationSearchServiceTests : IDisposable
{
    private readonly StandardAnalyzer _analyzer = new(LuceneVersion.LUCENE_48);

    private readonly RAMDirectory _directory = new();

    private readonly Fixture _fixture = new();

    private readonly ILuceneIndexerService<LocationDocument> _mockIndexerService = Substitute.For<ILuceneIndexerService<LocationDocument>>();

    private readonly ILogger<LocationSearchService> _mockLogger = Substitute.For<ILogger<LocationSearchService>>();

    public void Dispose()
    {
        _analyzer.Dispose();
        _directory.Dispose();
        _mockIndexerService.Dispose();
    }

    [Fact]
    public void Search_WhenNoMatches_ReturnsEmptyResults()
    {
        LocationDocument doc = CreateLocationDocument(
            "Metropolis",
            "New York",
            "NY",
            40.7128,
            -74.0060,
            ["10001", "10002",]
        );

        using SearcherManager searcherManager = CreateSearcherManager(doc);

        _mockIndexerService.SearcherManager.Returns(searcherManager);

        using LocationSearchService searchService = new(_mockIndexerService, _mockLogger);

        SearchResult<LocationDocument> result = searchService.Search("Gotham");

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    public void Search_WhenQueryIsTooShort_ReturnsEmptyResults(string query)
    {
        using SearcherManager searcherManager = CreateSearcherManager();

        _mockIndexerService.SearcherManager.Returns(searcherManager);

        using LocationSearchService searchService = new(_mockIndexerService, _mockLogger);

        SearchResult<LocationDocument> result = searchService.Search(query);

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Search_WhenSearchByLocalityWithSpaces_ReturnsMatchingDocuments()
    {
        LocationDocument doc = CreateLocationDocument(
            "Los Angeles",
            "California",
            "CA",
            34.0522,
            -118.2437,
            ["90001", "90002",]
        );

        using SearcherManager searcherManager = CreateSearcherManager(doc);

        _mockIndexerService.SearcherManager.Returns(searcherManager);

        using LocationSearchService searchService = new(_mockIndexerService, _mockLogger);

        SearchResult<LocationDocument> result = searchService.Search("Los Angeles");

        result.Results.Should().HaveCount(1);
        result.Results.Should().ContainEquivalentOf(doc);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void Search_WhenSearchingByLocalityPrefix_ReturnsMatchingDocuments()
    {
        LocationDocument doc1 = CreateLocationDocument(
            "Springfield",
            "Illinois",
            "IL",
            39.7817,
            -89.6501,
            ["62701", "62702",]
        );

        LocationDocument doc2 = CreateLocationDocument(
            "Shelbyville",
            "Illinois",
            "IL",
            39.4067,
            -88.7901,
            ["62565",]
        );

        using SearcherManager searcherManager = CreateSearcherManager(doc1, doc2);

        _mockIndexerService.SearcherManager.Returns(searcherManager);

        using LocationSearchService searchService = new(_mockIndexerService, _mockLogger);

        SearchResult<LocationDocument> result = searchService.Search("Spring");

        result.Results.Should().HaveCount(1);
        result.Results.Should().ContainEquivalentOf(doc1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void Search_WhenSearchingByPostalCode_ReturnsMatchingDocuments()
    {
        LocationDocument doc1 = CreateLocationDocument(
            "New York",
            "New York",
            "NY",
            40.7128,
            -74.0060,
            ["10001", "10002", "10003",]
        );

        LocationDocument doc2 = CreateLocationDocument(
            "Jersey City",
            "New Jersey",
            "NJ",
            40.7178,
            -74.0431,
            ["07302", "07303",]
        );

        LocationDocument doc3 = CreateLocationDocument(
            "Newark",
            "New Jersey",
            "NJ",
            40.7357,
            -74.1724,
            ["07101", "07102", "07103",]
        );

        using SearcherManager searcherManager = CreateSearcherManager(doc1, doc2, doc3);

        _mockIndexerService.SearcherManager.Returns(searcherManager);

        using LocationSearchService searchService = new(_mockIndexerService, _mockLogger);

        SearchResult<LocationDocument> result = searchService.Search("10001");

        result.Results.Should().HaveCount(1);
        result.Results.Should().ContainEquivalentOf(doc1);
        result.TotalCount.Should().Be(1);
    }

    private static Document CreateLuceneDocument(LocationDocument document)
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

    private LocationDocument CreateLocationDocument(
        string locality,
        string province,
        string provinceCode,
        double latitude,
        double longitude,
        params string[] postalCodes)
    {
        return new LocationDocument(
            _fixture.Create<Guid>(),
            locality,
            province,
            provinceCode,
            postalCodes,
            latitude,
            longitude
        );
    }

    private SearcherManager CreateSearcherManager(params LocationDocument[] documents)
    {
        #pragma warning disable CA2000 // IndexWriter is disposed by SearcherManager
        IndexWriter indexWriter = new(_directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer));
        #pragma warning restore CA2000

        indexWriter.AddDocuments([.. documents.Select(CreateLuceneDocument),]);
        indexWriter.Commit();

        return new SearcherManager(indexWriter, true, null);
    }
}
