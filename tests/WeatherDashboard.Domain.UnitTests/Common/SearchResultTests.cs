namespace WeatherDashboard.Domain.UnitTests.Common;

using AutoFixture;
using AwesomeAssertions;
using Domain.Common;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Search")]
[Trait("Speed", "Fast")]
public sealed class SearchResultTests
{
    private readonly Fixture _fixture = new();


    [Fact]
    public void Constructor_WithNegativeTotalCount_ShouldThrowArgumentOutOfRangeException()
    {
        List<string> results = _fixture.CreateMany<string>(5).ToList();
        const int totalCount = -1;

        Action act = () => _ = new SearchResult<string>(results, totalCount);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("TotalCount");
    }

    [Fact]
    public void Constructor_WithNullResults_ShouldThrowArgumentNullException()
    {
        List<string> results = null!;
        const int totalCount = 5;

        // ReSharper disable once ExpressionIsAlwaysNull
        Action act = () => _ = new SearchResult<string>(results!, totalCount);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        List<string> results = _fixture.CreateMany<string>(5).ToList();
        int totalCount = results.Count;

        SearchResult<string> searchResult = new(results, totalCount);

        searchResult.Results.Should().BeEquivalentTo(results);
        searchResult.TotalCount.Should().Be(totalCount);
    }

    [Fact]
    public void Constructor_WithZeroTotalCount_ShouldCreateInstance()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<string> results = [];
        const int totalCount = 0;

        SearchResult<string> searchResult = new(results, totalCount);

        searchResult.Results.Should().BeEmpty();
        searchResult.TotalCount.Should().Be(totalCount);
    }
}
