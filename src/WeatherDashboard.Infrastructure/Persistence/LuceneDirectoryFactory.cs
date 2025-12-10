namespace WeatherDashboard.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Lucene.Net.Store;

/// <summary>
///     Factory for creating and managing Lucene MMapDirectory instances.
/// </summary>
public sealed class LuceneDirectoryFactory : IDisposable
{
    private readonly ConcurrentDictionary<string, MMapDirectory> _directories = new(StringComparer.OrdinalIgnoreCase);

    private bool _isDisposed;

    /// <summary>
    ///     Releases all managed Lucene directory instances.
    /// </summary>
    public void Dispose()
    {
        if ( _isDisposed )
        {
            return;
        }

        foreach ( MMapDirectory directory in _directories.Values )
        {
            directory.Dispose();
        }

        _directories.Clear();
        _isDisposed = true;
    }

    /// <summary>
    ///     Gets or creates an MMapDirectory for the specified index directory path.
    /// </summary>
    /// <param name="indexDirectory">The directory information for the Lucene index.</param>
    /// <returns>An MMapDirectory instance for the specified path.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the factory has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexDirectory" /> is null.</exception>
    public MMapDirectory GetOrCreateDirectory(DirectoryInfo indexDirectory)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(indexDirectory);

        return _directories.GetOrAdd(indexDirectory.FullName, _ => new MMapDirectory(indexDirectory));
    }
}
