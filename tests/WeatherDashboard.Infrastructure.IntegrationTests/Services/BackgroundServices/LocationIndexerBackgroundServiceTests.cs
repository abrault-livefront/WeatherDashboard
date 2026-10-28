namespace WeatherDashboard.Infrastructure.IntegrationTests.Services.BackgroundServices;

using System.Reflection;
using System.Security.Cryptography;
using AwesomeAssertions;
using Domain.Entities.Documents;
using Infrastructure.Services.BackgroundServices;
using Infrastructure.Services.Indexer.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;

[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Slow")]
public sealed class LocationIndexerBackgroundServiceTests
{
    private const string s_resourceName = "WeatherDashboard.Infrastructure.Data.SeedData.Locations.json";

    private readonly IElasticsearchIndexerService<LocationDocument> _mockIndexerService;

    private readonly ILogger<LocationIndexerBackgroundService> _mockLogger;

    public LocationIndexerBackgroundServiceTests()
    {
        _mockIndexerService = Substitute.For<IElasticsearchIndexerService<LocationDocument>>();
        _mockLogger = Substitute.For<ILogger<LocationIndexerBackgroundService>>();

        _mockIndexerService.IndexName.Returns("locations");
    }

    [Fact]
    public void Constructor_WithNullIndexerService_ThrowsArgumentNullException()
    {
        Action act = () => _ = new LocationIndexerBackgroundService(null!, _mockLogger);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("indexerService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Action act = () => _ = new LocationIndexerBackgroundService(_mockIndexerService, null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    [Fact]
    public async Task ExecuteAsync_EnsuresIndexExists()
    {
        using LocationIndexerBackgroundService service = new(_mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        Func<Task> act = () => _mockIndexerService.Received(1).EnsureIndexExistsAsync(Arg.Any<CancellationToken>());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_SavesNewHash_AfterRebuild()
    {
        _mockIndexerService.GetSeedHashAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        using LocationIndexerBackgroundService service = new(_mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        Func<Task> act = () => _mockIndexerService.Received(1)
                                                  .SaveSeedHashAsync(Arg.Is<string>(h => !string.IsNullOrEmpty(h)), Arg.Any<CancellationToken>());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHashDoesNotMatch_RebuildsIndexAsync()
    {
        _mockIndexerService.GetSeedHashAsync(Arg.Any<CancellationToken>()).Returns("<OLD HASH>");

        using LocationIndexerBackgroundService service = new(_mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        Func<Task> act = async () =>
        {
            await _mockIndexerService.Received(1).RemoveAllAsync(Arg.Any<CancellationToken>()).ConfigureAwait(false);
            await _mockIndexerService.Received(1).IndexManyAsync(Arg.Any<IEnumerable<LocationDocument>>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
        };
        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHashMatches_DoesNotRebuildIndexAsync()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(LocationIndexerBackgroundService))!;

        await using Stream stream = assembly.GetManifestResourceStream(s_resourceName)!;

        byte[] data = new byte[stream.Length];

        await stream.ReadExactlyAsync(data, TestContext.Current.CancellationToken);

        string hash = Convert.ToHexString(SHA256.HashData(data));

        _mockIndexerService.GetSeedHashAsync(Arg.Any<CancellationToken>()).Returns(hash);

        using LocationIndexerBackgroundService service = new(_mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        Func<Task> act = async () =>
        {
            await _mockIndexerService.DidNotReceive().RemoveAllAsync(Arg.Any<CancellationToken>()).ConfigureAwait(false);
            await _mockIndexerService.DidNotReceive().IndexManyAsync(Arg.Any<IEnumerable<LocationDocument>>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            await _mockIndexerService.DidNotReceive().SaveSeedHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
        };
        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSeedHashRecorded_RebuildsIndexAsync()
    {
        _mockIndexerService.GetSeedHashAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        using LocationIndexerBackgroundService service = new(_mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        Func<Task> act = async () =>
        {
            await _mockIndexerService.Received(1).RemoveAllAsync(Arg.Any<CancellationToken>()).ConfigureAwait(false);
            await _mockIndexerService.Received(1).IndexManyAsync(Arg.Any<IEnumerable<LocationDocument>>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
        };
        
        await act.Should().NotThrowAsync();
    }
}
