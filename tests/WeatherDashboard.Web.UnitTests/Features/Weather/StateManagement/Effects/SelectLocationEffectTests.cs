namespace WeatherDashboard.Web.UnitTests.Features.Weather.StateManagement.Effects;

using AutoFixture.Xunit3;
using AwesomeAssertions;
using Domain.Entities.Documents;
using Fluxor;
using NSubstitute;
using Web.Features.Weather.StateManagement.Actions;
using Web.Features.Weather.StateManagement.Effects;

[Trait("Category", "Unit")]
[Trait("Layer", "Web")]
[Trait("Feature", "StateManagement")]
[Trait("Component", "Effect")]
[Trait("Speed", "Fast")]
public sealed class SelectLocationEffectTests
{
    private readonly IDispatcher _mockDispatcher = Substitute.For<IDispatcher>();

    [Fact]
    public void Constructor_CreatesEffect()
    {
        SelectLocationEffect effect = new();

        effect.Should().NotBeNull();
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_CompletesSuccessfully(LocationDocument location)
    {
        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);

        Func<Task> act = async () => await effect.HandleAsync(action, _mockDispatcher).ConfigureAwait(true);

        await act.Should().NotThrowAsync().ConfigureAwait(true);
    }

    [Fact]
    public async Task HandleAsync_WithNullAction_ThrowsArgumentNullException()
    {
        SelectLocationEffect effect = new();
        SelectLocationAction? action = null;

        Func<Task> act = async () => await effect.HandleAsync(action!, _mockDispatcher).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("action")
                 .ConfigureAwait(true);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithNullDispatcher_ThrowsArgumentNullException(LocationDocument location)
    {
        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);
        IDispatcher? dispatcher = null;

        Func<Task> act = async () => await effect.HandleAsync(action, dispatcher!).ConfigureAwait(true);

        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("dispatcher")
                 .ConfigureAwait(true);
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidAction_DispatchesBothActions(LocationDocument location)
    {
        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(2).Dispatch(Arg.Any<object>());
        _mockDispatcher.Received(1).Dispatch(Arg.Any<LocationSelectedAction>());
        _mockDispatcher.Received(1).Dispatch(Arg.Any<FetchWeatherAction>());
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidAction_DispatchesFetchWeatherAction(LocationDocument location)
    {
        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<FetchWeatherAction>(a => a.Latitude.Equals(location.Latitude)
                                                                          && a.Longitude.Equals(location.Longitude)));
    }

    [Theory]
    [AutoData]
    public async Task HandleAsync_WithValidAction_DispatchesLocationSelectedAction(LocationDocument location)
    {
        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<LocationSelectedAction>(a => a.Location == location));
    }

    [Theory]
    [InlineAutoData("New York", "New York", "NY", 40.7128, -74.0060)]
    [InlineAutoData("Los Angeles", "California", "CA", 34.0522, -118.2437)]
    [InlineAutoData("London", "England", "ENG", 51.5074, -0.1278)]
    [InlineAutoData("Tokyo", "Tokyo", "TKY", 35.6762, 139.6503)]
    public async Task HandleAsync_WithVariousLocations_DispatchesActionsWithCorrectCoordinates(
        string locality,
        string province,
        string provinceCode,
        double latitude,
        double longitude)
    {
        LocationDocument location = new(
            Guid.NewGuid(),
            locality,
            province,
            provinceCode,
            (List<string>)
            [
                "12345",
            ],
            latitude,
            longitude);

        SelectLocationEffect effect = new();
        SelectLocationAction action = new(location);

        await effect.HandleAsync(action, _mockDispatcher);

        _mockDispatcher.Received(1).Dispatch(Arg.Is<FetchWeatherAction>(a => a.Latitude.Equals(latitude)
                                                                          && a.Longitude.Equals(longitude)));

        _mockDispatcher.Received(1).Dispatch(Arg.Is<LocationSelectedAction>(a => a.Location == location));
    }
}
