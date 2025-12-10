namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Effects;

using Application.Common.Interfaces;
using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Common;
using Domain.Entities.Documents;
using Fluxor;
using NSubstitute;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Effects;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "Search")]
[Trait("Component", "Effect")]
[Trait("Speed", "Fast")]
public sealed class SearchLocationEffectTests
{
    private readonly IDispatcher _mockDispatcher = Substitute.For<IDispatcher>();

    private readonly ISearchService<LocationDocument> _mockSearchService = Substitute.For<ISearchService<LocationDocument>>();

    [Fact]
    public void Constructor_WithNullSearchService_ThrowsArgumentNullException()
    {
        ISearchService<LocationDocument>? searchService = null;

        #pragma warning disable CA1806
        Action act = () => _ = new SearchLocationEffect(searchService!);
        #pragma warning restore CA1806

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("searchService");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesEffect()
    {
        SearchLocationEffect effect = new(_mockSearchService);

        effect.Should().NotBeNull();
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_CompletesSuccessfully(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(query).Returns(searchResult);

        Func<Task> act = async () => await effect.HandleAsync(action, _mockDispatcher).ConfigureAwait(true);

        await act.Should().NotThrowAsync().ConfigureAwait(true);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyQuery_DispatchesEmptyResults()
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(string.Empty);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 0));
        _mockSearchService.DidNotReceive().Search(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WithNullAction_ThrowsArgumentNullException()
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction? action = null;

        Func<Task> act = async () => await effect.HandleAsync(action!, _mockDispatcher).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("action")
                 .ConfigureAwait(true);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithNullDispatcher_ThrowsArgumentNullException(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        IDispatcher? dispatcher = null;

        Func<Task> act = async () => await effect.HandleAsync(action, dispatcher!).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("dispatcher")
                 .ConfigureAwait(true);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_DispatchesEmptyResults()
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(null!);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 0));
        _mockSearchService.DidNotReceive().Search(Arg.Any<string>());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("New")]
    [InlineData("NYC")]
    public async Task HandleAsync_WithQueryExactlyMinimumLength_CallsSearchService(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(query).Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockSearchService.Received(1).Search(query);
        _mockDispatcher.Received(1).Dispatch(Arg.Any<SearchLocationSuccessAction>());
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("NY")]
    public async Task HandleAsync_WithQueryLessThanMinimumLength_DispatchesEmptyResults(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 0));
        _mockSearchService.DidNotReceive().Search(Arg.Any<string>());
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithSearchServiceReturningEmptyResults_DispatchesEmptyResults(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(query).Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 0));
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithSearchServiceReturningMultipleResults_DispatchesAllResults(
        LocationDocument location1,
        LocationDocument location2,
        LocationDocument location3)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new("test query");
        IReadOnlyList<LocationDocument> locations = [location1, location2, location3,];
        SearchResult<LocationDocument> searchResult = new(locations, 3);

        _mockSearchService.Search("test query").Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 3
                                                                                   && a.Results.Contains(location1)
                                                                                   && a.Results.Contains(location2)
                                                                                   && a.Results.Contains(location3)));
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidQuery_CallsSearchService(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(query).Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockSearchService.Received(1).Search(query);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidQuery_DispatchesSearchResults(LocationDocument location)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new("New York");
        IReadOnlyList<LocationDocument> locations = [location,];
        SearchResult<LocationDocument> searchResult = new(locations, 1);

        _mockSearchService.Search("New York").Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 1 && a.Results.Contains(location)));
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidQuery_PassesQueryExactlyAsProvided(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Ensure query is long enough
        string validQuery = query.Length >= 3 ? query : query + "abc";

        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(validQuery);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(validQuery).Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        // Verify the exact query was passed (no trimming, case changes, etc.)
        _mockSearchService.Received(1).Search(validQuery);
    }

    [Theory]
    [InlineData("New York")]
    [InlineData("Los Angeles")]
    [InlineData("Chicago")]
    [InlineData("Houston")]
    [InlineData("Phoenix")]
    public async Task HandleAsync_WithVariousValidQueries_CallsSearchServiceWithCorrectQuery(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);
        SearchResult<LocationDocument> searchResult = new([], 0);

        _mockSearchService.Search(query).Returns(searchResult);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockSearchService.Received(1).Search(query);
        _mockDispatcher.Received(1).Dispatch(Arg.Any<SearchLocationSuccessAction>());
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   \t  \n  ")]
    public async Task HandleAsync_WithWhitespaceQuery_DispatchesEmptyResults(string query)
    {
        SearchLocationEffect effect = new(_mockSearchService);
        SearchLocationAction action = new(query);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<SearchLocationSuccessAction>(a => a.Results.Count == 0));
        _mockSearchService.DidNotReceive().Search(Arg.Any<string>());
    }
}
