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
public sealed class LocationIndexerBackgroundServiceTests : IDisposable
{
    private readonly DirectoryInfo _indexDirectoryPath;

    private readonly ILuceneIndexerService<LocationDocument> _mockIndexerService;

    private readonly ILogger<LocationIndexerBackgroundService> _mockLogger;

    private readonly DirectoryInfo _tempDirectoryPath;

    public LocationIndexerBackgroundServiceTests()
    {
        _mockIndexerService = Substitute.For<ILuceneIndexerService<LocationDocument>>();
        _mockLogger = Substitute.For<ILogger<LocationIndexerBackgroundService>>();

        _tempDirectoryPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        _indexDirectoryPath = new DirectoryInfo(Path.Combine(_tempDirectoryPath.FullName, "Index"));

        _mockIndexerService.IndexName.Returns("locations");
        _mockIndexerService.IndexDirectory.Returns(_indexDirectoryPath);
    }

    [Fact]
    public void Constructor_WithNullIndexDirectory_ThrowsArgumentNullException()
    {
        Action act = () => _ = new LocationIndexerBackgroundService(null!, _mockIndexerService, _mockLogger);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("indexDirectory");
    }

    [Fact]
    public void Constructor_WithNullIndexerService_ThrowsArgumentNullException()
    {
        Action act = () => _ = new LocationIndexerBackgroundService(_indexDirectoryPath, null!, _mockLogger);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("indexerService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Action act = () => _ = new LocationIndexerBackgroundService(_indexDirectoryPath, _mockIndexerService, null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    public void Dispose()
    {
        _mockIndexerService.Dispose();

        if ( !_tempDirectoryPath.Exists )
        {
            return;
        }

        try
        {
            _tempDirectoryPath.Delete(true);
        }
        catch
        {
            // Ignore exceptions during cleanup
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIndexDirectory_WhenItDoesNotExist()
    {
        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        _indexDirectoryPath.Exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_CreatesMetadataDirectory_WhenItDoesNotExist()
    {
        DirectoryInfo metadataDirectory = new(Path.Combine(_indexDirectoryPath.FullName, "Metadata"));

        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        metadataDirectory.Exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_SavesNewHash_AfterRebuild()
    {
        DirectoryInfo metadataDirectory = new(Path.Combine(_indexDirectoryPath.FullName, "Metadata"));

        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        FileInfo hashFile = new(Path.Combine(metadataDirectory.FullName, "locations.version"));

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        hashFile.Exists.Should().BeTrue();
        string savedHash = await File.ReadAllTextAsync(hashFile.FullName, TestContext.Current.CancellationToken);
        savedHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHashDoesNotMatch_RebuildsIndexAsync()
    {
        DirectoryInfo metadataDirectory = new(Path.Combine(_indexDirectoryPath.FullName, "Metadata"));
        metadataDirectory.Create();

        FileInfo hashFile = new(Path.Combine(metadataDirectory.FullName, "locations.version"));
        await File.WriteAllTextAsync(hashFile.FullName, "<OLD HASH>", TestContext.Current.CancellationToken);

        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        _mockIndexerService.Received(1).RemoveAll();
        _mockIndexerService.Received(1).IndexMany(Arg.Any<IEnumerable<LocationDocument>>());
        _mockIndexerService.Received(1).Optimize();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHashFileDoesNotExist_RebuildsIndexAsync()
    {
        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        _mockIndexerService.Received(1).RemoveAll();
        _mockIndexerService.Received(1).IndexMany(Arg.Any<IEnumerable<LocationDocument>>());
        _mockIndexerService.Received(1).Optimize();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHashMatches_DoesNotRebuildIndexAsync()
    {
        const string resourceName = "WeatherDashboard.Infrastructure.Data.SeedData.Locations.json";

        DirectoryInfo metadataDirectory = new(Path.Combine(_indexDirectoryPath.FullName, "Metadata"));
        metadataDirectory.Create();

        FileInfo hashFile = new(Path.Combine(metadataDirectory.FullName, "locations.version"));

        Assembly? assembly = Assembly.GetAssembly(typeof(LocationIndexerBackgroundService));

        await using ( Stream stream = assembly!.GetManifestResourceStream(resourceName)! )
        {
            byte[] data = new byte[stream!.Length];

            await stream.ReadExactlyAsync(data, TestContext.Current.CancellationToken);

            string hash = Convert.ToHexString(SHA256.HashData(data));

            await File.WriteAllTextAsync(hashFile.FullName, hash, TestContext.Current.CancellationToken);
        }

        using LocationIndexerBackgroundService service = new(_indexDirectoryPath, _mockIndexerService, _mockLogger);

        await service.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken); // Allow some time for the background service to run
        await service.StopAsync(TestContext.Current.CancellationToken);

        _mockIndexerService.DidNotReceive().RemoveAll();
        _mockIndexerService.DidNotReceive().IndexMany(Arg.Any<IEnumerable<LocationDocument>>());
        _mockIndexerService.DidNotReceive().Optimize();
    }
}
