namespace WeatherDashboard.Infrastructure.Configuration;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Configuration options for Redis connection and behavior.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RedisOptions
{
    /// <summary>
    ///     Configuration section name for cache-specific Redis settings.
    /// </summary>
    /// <remarks>
    ///     Use this section to bind options related to application-level caching.
    /// </remarks>
    public const string SectionNameCache = "RedisCache";

    /// <summary>
    ///     Configuration section name for general Redis connection and behavior settings.
    /// </summary>
    /// <remarks>
    ///     Use this section to bind options related to application-level configurations.
    /// </remarks>
    public const string SectionNameConfig = "RedisConfig";

    /// <summary>
    ///     Gets or initializes the connection timeout in milliseconds.
    /// </summary>
    /// <value>
    ///     The maximum time in milliseconds to wait for a connection to be established.
    ///     Default is 5000 milliseconds (5 seconds).
    /// </value>
    public int ConnectTimeoutMilliseconds { get; init; } = 5000;

    /// <summary>
    ///     Gets or initializes the collection of Redis endpoint configurations.
    /// </summary>
    /// <value>
    ///     A read-only list of Redis endpoints to connect to. Supports multiple endpoints for cluster or replica
    ///     configurations.
    ///     Default is an empty collection.
    /// </value>
    public IReadOnlyList<RedisEndPointOptions> EndPoints { get; init; } = [];

    /// <summary>
    ///     Gets or initializes a value indicating whether DNS resolution should be performed for endpoints.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to resolve DNS names to IP addresses before connecting; otherwise, <see langword="false" />
    ///     Default is <see langword="false" />.
    /// </value>
    public bool ResolveDns { get; init; }

    /// <summary>
    ///     Gets or initializes the synchronous operation timeout in milliseconds.
    /// </summary>
    /// <value>
    ///     The maximum time in milliseconds to wait for synchronous Redis operations to complete.
    ///     Default is 5000 milliseconds (5 seconds).
    /// </value>
    public int SyncTimeoutMilliseconds { get; init; } = 5000;
}
