namespace WeatherDashboard.Infrastructure.IntegrationTests.Persistence;

using AwesomeAssertions;
using Infrastructure.Persistence;
using Lucene.Net.Store;
using Directory = Directory;

[Trait("Category", "Integration")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Slow")]
public sealed class LuceneDirectoryFactoryTests : IDisposable
{
    private readonly string _tempDirectoryPath;

    public LuceneDirectoryFactoryTests()
    {
        _tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_tempDirectoryPath);
    }

    public void Dispose()
    {
        if ( !Directory.Exists(_tempDirectoryPath) )
        {
            return;
        }

        try
        {
            Directory.Delete(_tempDirectoryPath, true);
        }
        catch
        {
            // Ignore exceptions during cleanup
        }
    }

    [Fact]
    public void Dispose_ShouldReleaseAllDirectories()
    {
        LuceneDirectoryFactory factory = new();
        DirectoryInfo directoryInfo1 = new(Path.Combine(_tempDirectoryPath, "Index1"));
        DirectoryInfo directoryInfo2 = new(Path.Combine(_tempDirectoryPath, "Index2"));

        MMapDirectory dir1 = factory.GetOrCreateDirectory(directoryInfo1);
        MMapDirectory dir2 = factory.GetOrCreateDirectory(directoryInfo2);

        factory.Dispose();

        Action act1 = () => dir1.ListAll();
        Action act2 = () => dir2.ListAll();

        act1.Should().Throw<ObjectDisposedException>();
        act2.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void GetOrCreateDirectory_AfterDispose_ShouldThrowObjectDisposedException()
    {
        LuceneDirectoryFactory factory = new();
        DirectoryInfo directoryInfo = new(_tempDirectoryPath);

        factory.Dispose();

        Func<MMapDirectory> act = () => factory.GetOrCreateDirectory(directoryInfo);

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void GetOrCreateDirectory_WithDifferentCasing_ShouldReturnSameInstance()
    {
        using LuceneDirectoryFactory factory = new();
        string upperCasePath = _tempDirectoryPath.ToUpperInvariant();
        string lowerCasePath = _tempDirectoryPath.ToLowerInvariant();

        DirectoryInfo upperCaseDirectoryInfo = new(upperCasePath);
        DirectoryInfo lowerCaseDirectoryInfo = new(lowerCasePath);

        MMapDirectory firstInstance = factory.GetOrCreateDirectory(upperCaseDirectoryInfo);
        MMapDirectory secondInstance = factory.GetOrCreateDirectory(lowerCaseDirectoryInfo);

        secondInstance.Should().BeSameAs(firstInstance);
    }

    [Fact]
    public void GetOrCreateDirectory_WithNullDirectory_ShouldThrowArgumentNullException()
    {
        using LuceneDirectoryFactory factory = new();

        // ReSharper disable once AccessToDisposedClosure
        Action act = () => factory.GetOrCreateDirectory(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("indexDirectory");
    }

    [Fact]
    public void GetOrCreateDirectory_WithSamePath_ShouldReturnSameInstance()
    {
        using LuceneDirectoryFactory factory = new();
        DirectoryInfo directoryInfo = new(_tempDirectoryPath);

        MMapDirectory firstInstance = factory.GetOrCreateDirectory(directoryInfo);
        MMapDirectory secondInstance = factory.GetOrCreateDirectory(directoryInfo);

        secondInstance.Should().BeSameAs(firstInstance);
    }

    [Fact]
    public void GetOrCreateDirectory_WithValidDirectory_ShouldReturnMMapDirectory()
    {
        using LuceneDirectoryFactory factory = new();
        DirectoryInfo directoryInfo = new(_tempDirectoryPath);

        MMapDirectory result = factory.GetOrCreateDirectory(directoryInfo);

        result.Should().NotBeNull();
        result.Should().BeOfType<MMapDirectory>();
    }
}
