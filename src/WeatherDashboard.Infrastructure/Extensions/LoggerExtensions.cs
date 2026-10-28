namespace WeatherDashboard.Infrastructure.Extensions;

using Microsoft.Extensions.Logging;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Critical,
        "Failed to create index '{IndexName}': {Reason}")]
    public static partial void LogFailedToCreateIndex(this ILogger logger, string indexName, string reason);

    [LoggerMessage(LogLevel.Critical,
        "Failed to delete index '{IndexName}': {Reason}")]
    public static partial void LogFailedToDeleteIndex(this ILogger logger, string indexName, string reason);

    [LoggerMessage(LogLevel.Critical,
        "Failed to index documents in '{IndexName}': {Reason}")]
    public static partial void LogFailedToIndexDocuments(this ILogger logger, string indexName, string reason);

    [LoggerMessage(LogLevel.Error,
        "Failed to search index '{IndexName}': {Reason}")]
    public static partial void LogFailedToSearch(this ILogger logger, string indexName, string reason);

    [LoggerMessage(LogLevel.Critical,
        "Resource '{ResourceName}' could not be located within assembly '{AssemblyName}'")]
    public static partial void LogResourceCouldNotBeLocated(this ILogger logger, string resourceName, string? assemblyName);
}
