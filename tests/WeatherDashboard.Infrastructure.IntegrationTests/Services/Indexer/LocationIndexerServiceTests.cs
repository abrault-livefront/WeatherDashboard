namespace WeatherDashboard.Infrastructure.IntegrationTests.Services.Indexer;

using System.Globalization;
using AutoFixture;
using AwesomeAssertions;
using Domain.Entities.Documents;
using Infrastructure.Persistence;
using Infrastructure.Services.Indexer;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;
using NSubstitute;

[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Slow")]
public sealed class LocationIndexerServiceTests : IDisposable
{
    private readonly LuceneDirectoryFactory _directoryFactory;

    private readonly Fixture _fixture = new();

    private readonly LocationIndexerService _indexerService;

    public LocationIndexerServiceTests()
    {
        ILogger<LocationIndexerService> mockLogger = Substitute.For<ILogger<LocationIndexerService>>();

        DirectoryInfo tempDirectoryPath = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        DirectoryInfo indexDirectoryPath = new(Path.Combine(tempDirectoryPath.FullName, "Index"));

        _directoryFactory = new LuceneDirectoryFactory();
        _indexerService = new LocationIndexerService(indexDirectoryPath, mockLogger, _directoryFactory);
    }

    public void Dispose()
    {
        _indexerService.Dispose();
        _directoryFactory.Dispose();
    }

    [Fact]
    public async Task Index_ShouldIndexDocument_WithAllFields()
    {
        LocationDocument document = _fixture.Build<LocationDocument>()
                                            .With(w =>
                                                 w.PostalCodes, _fixture.CreateMany<string>(2)
                                                                        .ToList()
                                                                        .AsReadOnly())
                                            .Create();

        _indexerService.Index(document);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        Document? results = await SearchByIdAsync(document.Id);

        results.Should().NotBeNull();
        results.Get(nameof(LocationDocument.Locality), CultureInfo.InvariantCulture).Should().Be(document.Locality);
        results.Get(nameof(LocationDocument.Province), CultureInfo.InvariantCulture).Should().Be(document.Province);
    }

    [Fact]
    public async Task Index_ShouldIndexMultiplePostalCodes()
    {
        HashSet<string> postalCodes = _fixture.CreateMany<string>(3)
                                              .ToHashSet(StringComparer.OrdinalIgnoreCase);

        LocationDocument document = _fixture.Build<LocationDocument>()
                                            .With(w => w.PostalCodes, postalCodes
                                                                     .ToList()
                                                                     .AsReadOnly())
                                            .Create();

        _indexerService.Index(document);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        Document? results = await SearchByIdAsync(document.Id);

        results.Should().NotBeNull();

        string[]? indexedPostalCodes = results.GetValues(nameof(LocationDocument.PostalCodes), CultureInfo.InvariantCulture);

        indexedPostalCodes.Should().BeEquivalentTo(postalCodes);
    }

    [Fact]
    public async Task Index_ShouldStoreLatitudeAndLongitude()
    {
        LocationDocument? document = _fixture.Build<LocationDocument>()
                                             .With(w => w.Latitude, _fixture.Create<double>())
                                             .With(w => w.Longitude, _fixture.Create<double>())
                                             .Create();

        _indexerService.Index(document);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        Document? results = await SearchByIdAsync(document.Id);

        results.Should().NotBeNull();

        results.GetField(nameof(LocationDocument.Latitude)).GetDoubleValue().Should().Be(document.Latitude);
        results.GetField(nameof(LocationDocument.Longitude)).GetDoubleValue().Should().Be(document.Longitude);
    }

    [Fact]
    public async Task Index_ShouldUpdateExistingDocument()
    {
        string locality = _fixture.Create<string>();

        LocationDocument? document = _fixture.Create<LocationDocument>();

        _indexerService.Index(document);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        LocationDocument updatedDocument = document with
        {
            Locality = locality,
        };

        _indexerService.Index(updatedDocument);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        Document? results = await SearchByIdAsync(document.Id);

        results.Should().NotBeNull();
        results.Get(nameof(LocationDocument.Locality), CultureInfo.InvariantCulture).Should().Be(locality);
    }

    [Fact]
    public async Task IndexMany_ShouldIndexMultipleDocuments()
    {
        List<LocationDocument> documents = _fixture.CreateMany<LocationDocument>(5).ToList();

        _indexerService.IndexMany(documents);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        foreach ( LocationDocument document in documents )
        {
            Document? results = await SearchByIdAsync(document.Id);

            results.Should().NotBeNull();
            results.Get(nameof(LocationDocument.Locality), CultureInfo.InvariantCulture).Should().Be(document.Locality);
        }
    }

    [Fact]
    public async Task Optimize_ShouldPreserveIndexedDocuments()
    {
        List<LocationDocument> documents = _fixture.CreateMany<LocationDocument>(5).ToList();

        _indexerService.IndexMany(documents);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        foreach ( LocationDocument document in documents )
        {
            ( await SearchByIdAsync(document.Id) ).Should().NotBeNull();
        }

        _indexerService.Optimize();

        foreach ( LocationDocument document in documents )
        {
            Document? result = await SearchByIdAsync(document.Id);

            result.Should().NotBeNull();
            result.Get(nameof(LocationDocument.Locality), CultureInfo.InvariantCulture).Should().Be(document.Locality);
        }
    }

    [Fact]
    public async Task RemoveAll_ShouldClearAllIndexedDocuments()
    {
        List<LocationDocument> documents = _fixture.CreateMany<LocationDocument>(5).ToList();

        _indexerService.IndexMany(documents);
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        foreach ( LocationDocument document in documents )
        {
            ( await SearchByIdAsync(document.Id) ).Should().NotBeNull();
        }

        _indexerService.RemoveAll();
        _indexerService.SearcherManager.IsSearcherCurrent().Should().BeTrue();

        foreach ( LocationDocument document in documents )
        {
            ( await SearchByIdAsync(document.Id) ).Should().BeNull();
        }
    }

    private async Task<Document?> SearchByIdAsync(Guid id)
    {
        _indexerService.SearcherManager.MaybeRefreshBlocking();

        IndexSearcher? searcher = null;
        try
        {
            searcher = _indexerService.SearcherManager.Acquire();

            TermQuery query = new(new Term("Id", id.ToString()));
            TopDocs hits = searcher.Search(query, 1);

            return hits.TotalHits > 0
                       ? await Task.FromResult<Document?>(searcher.Doc(hits.ScoreDocs[0].Doc)).ConfigureAwait(false)
                       : await Task.FromResult<Document?>(null).ConfigureAwait(false);
        }
        finally
        {
            if ( searcher is not null )
            {
                _indexerService.SearcherManager.Release(searcher);
            }
        }
    }
}
