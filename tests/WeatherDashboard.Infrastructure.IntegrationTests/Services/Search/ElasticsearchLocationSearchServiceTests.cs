namespace WeatherDashboard.Infrastructure.IntegrationTests.Services.Search;

using AwesomeAssertions;
using Domain.Common;
using Domain.Entities.Documents;
using Infrastructure.Services.Indexer;
using Infrastructure.Services.Search;
using Microsoft.Extensions.Logging;
using NSubstitute;

[Collection("Elasticsearch")]
[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Slow")]
public sealed class ElasticsearchLocationSearchServiceTests
{
    private readonly ElasticsearchLocationIndexerService _indexer;

    private readonly ElasticsearchLocationSearchService _sut;

    public ElasticsearchLocationSearchServiceTests(ElasticsearchContainerFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _indexer = new ElasticsearchLocationIndexerService(fixture.Client,
            Substitute.For<ILogger<ElasticsearchLocationIndexerService>>());
        _sut = new ElasticsearchLocationSearchService(fixture.Client, Substitute.For<ILogger<ElasticsearchLocationSearchService>>());
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        LocationDocument[] locations =
        [
            new(Guid.NewGuid(), "Springfield", "Illinois", "IL", ["62701",], 39.7817, -89.6501),
            new(Guid.NewGuid(), "Springfield", "Massachusetts", "MA", ["01101",], 42.1015, -72.5898),
            new(Guid.NewGuid(), "Springfield", "Missouri", "MO", ["65801",], 37.2090, -93.2923),
        ];

        await SeedAsync(locations);

        SearchResult<LocationDocument> result =
            await _sut.SearchAsync("springfield", 1, TestContext.Current.CancellationToken);

        result.Results.Should().HaveCount(1);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ReturnsEmptyResult()
    {
        await SeedAsync(new LocationDocument(Guid.NewGuid(), "Austin", "Texas", "TX", ["78701",], 30.2672, -97.7431));

        SearchResult<LocationDocument> result =
            await _sut.SearchAsync("zzznomatch", cancellationToken: TestContext.Current.CancellationToken);

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithPartialLocality_ReturnsMatchingLocation()
    {
        LocationDocument austin = new(Guid.NewGuid(), "Austin", "Texas", "TX", ["78701",], 30.2672, -97.7431);
        LocationDocument boston = new(Guid.NewGuid(), "Boston", "Massachusetts", "MA", ["02108",], 42.3601, -71.0589);

        await SeedAsync(austin, boston);

        SearchResult<LocationDocument> result = await _sut.SearchAsync("aus", cancellationToken: TestContext.Current.CancellationToken);

        result.Results.Should().ContainSingle(l => l.Id == austin.Id);
    }

    [Fact]
    public async Task SearchAsync_WithPartialPostalCode_ReturnsMatchingLocation_RegardlessOfCase()
    {
        LocationDocument austin = new(Guid.NewGuid(), "Austin", "Texas", "TX", ["78701",], 30.2672, -97.7431);

        await SeedAsync(austin);

        SearchResult<LocationDocument> result = await _sut.SearchAsync("787", cancellationToken: TestContext.Current.CancellationToken);

        result.Results.Should().ContainSingle(l => l.Id == austin.Id);
    }

    [Fact]
    public async Task SearchAsync_WithQueryShorterThanMinimumLength_ReturnsEmptyResult()
    {
        await SeedAsync(new LocationDocument(Guid.NewGuid(), "Austin", "Texas", "TX", ["78701",], 30.2672, -97.7431));

        SearchResult<LocationDocument> result = await _sut.SearchAsync("au", cancellationToken: TestContext.Current.CancellationToken);

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithWildcardCharacters_StripsThemBeforeSearching()
    {
        LocationDocument austin = new(Guid.NewGuid(), "Austin", "Texas", "TX", ["78701",], 30.2672, -97.7431);

        await SeedAsync(austin);

        SearchResult<LocationDocument> result = await _sut.SearchAsync("au*stin", cancellationToken: TestContext.Current.CancellationToken);

        result.Results.Should().ContainSingle(l => l.Id == austin.Id);
    }

    private async Task SeedAsync(params LocationDocument[] locations)
    {
        await _indexer.RemoveAllAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        await _indexer.IndexManyAsync(locations, TestContext.Current.CancellationToken).ConfigureAwait(false);
    }
}
