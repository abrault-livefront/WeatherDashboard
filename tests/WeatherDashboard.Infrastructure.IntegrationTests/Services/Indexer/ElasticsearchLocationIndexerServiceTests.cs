namespace WeatherDashboard.Infrastructure.IntegrationTests.Services.Indexer;

using AwesomeAssertions;
using Domain.Entities.Documents;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Services.Indexer;
using Microsoft.Extensions.Logging;
using NSubstitute;

[Collection("Elasticsearch")]
[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Slow")]
public sealed class ElasticsearchLocationIndexerServiceTests
{
    private const string s_metadataIndexName = "locations-metadata";

    private readonly ElasticsearchClient _client;

    private readonly ElasticsearchLocationIndexerService _sut;

    public ElasticsearchLocationIndexerServiceTests(ElasticsearchContainerFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        _client = fixture.Client;
        _sut = new ElasticsearchLocationIndexerService(_client, Substitute.For<ILogger<ElasticsearchLocationIndexerService>>());
    }

    [Fact]
    public async Task EnsureIndexExistsAsync_WhenCalledTwice_DoesNotThrow()
    {
        await _sut.RemoveAllAsync(TestContext.Current.CancellationToken);

        Func<Task> act = async () => await _sut.EnsureIndexExistsAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetSeedHashAsync_WhenNeverSeeded_ReturnsNull()
    {
        await _client.Indices.DeleteAsync(s_metadataIndexName, TestContext.Current.CancellationToken);

        string? hash = await _sut.GetSeedHashAsync(TestContext.Current.CancellationToken);

        hash.Should().BeNull();
    }

    [Fact]
    public async Task IndexAsync_ThenRemoveAllAsync_LeavesIndexEmpty()
    {
        await _sut.RemoveAllAsync(TestContext.Current.CancellationToken);

        LocationDocument location = CreateLocation();
        await _sut.IndexAsync(location, TestContext.Current.CancellationToken);

        await _sut.RemoveAllAsync(TestContext.Current.CancellationToken);

        GetResponse<LocationDocument> response = await _client.GetAsync<LocationDocument>(_sut.IndexName,
                                                     location.Id.ToString(),
                                                     TestContext.Current.CancellationToken);

        response.Found.Should().BeFalse();
    }

    [Fact]
    public async Task IndexManyAsync_UpsertsById_WhenDocumentIndexedTwiceWithSameId()
    {
        await _sut.RemoveAllAsync(TestContext.Current.CancellationToken);

        LocationDocument original = CreateLocation();
        LocationDocument updated = original with
        {
            Locality = "Shelbyville",
        };

        await _sut.IndexManyAsync([original,], TestContext.Current.CancellationToken);
        await _sut.IndexManyAsync([updated,], TestContext.Current.CancellationToken);

        GetResponse<LocationDocument> response = await _client.GetAsync<LocationDocument>(_sut.IndexName,
                                                     original.Id.ToString(),
                                                     TestContext.Current.CancellationToken);

        response.Found.Should().BeTrue();
        response.Source.Should().NotBeNull();
        response.Source!.Locality.Should().Be("Shelbyville");
    }

    [Fact]
    public async Task SeedHash_RoundTrips_WhenSaved()
    {
        string hash = Guid.NewGuid().ToString("N");

        await _sut.SaveSeedHashAsync(hash, TestContext.Current.CancellationToken);

        string? savedHash = await _sut.GetSeedHashAsync(TestContext.Current.CancellationToken);

        savedHash.Should().Be(hash);
    }

    private static LocationDocument CreateLocation(string locality = "Springfield", string provinceCode = "IL")
    {
        return new LocationDocument(Guid.NewGuid(), locality, "Illinois", provinceCode, ["62701",], 39.7817, -89.6501);
    }
}
