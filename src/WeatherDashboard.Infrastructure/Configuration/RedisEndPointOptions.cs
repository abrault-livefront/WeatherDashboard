namespace WeatherDashboard.Infrastructure.Configuration;

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

/// <summary>
///     Configuration options for a Redis endpoint.
/// </summary>
[ExcludeFromCodeCoverage]
[UsedImplicitly]
public sealed class RedisEndPointOptions
{
    /// <summary>
    ///     Gets or initializes the Redis server host.
    /// </summary>
    /// <value>
    ///     The hostname or IP address of the Redis server.
    ///     Default is "localhost".
    /// </value>
    public string Host { get; init; } = "localhost";

    /// <summary>
    ///     Gets or initializes the Redis server port.
    /// </summary>
    /// <value>
    ///     The port number of the Redis server.
    ///     Default is 6379.
    /// </value>
    public int Port { get; init; } = 6379;
}
