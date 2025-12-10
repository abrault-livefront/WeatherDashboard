namespace WeatherDashboard.Infrastructure.Extensions;

using Microsoft.Extensions.Logging;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Critical,
        "An exception was thrown indicating that {IndexName} is in an inconsistent state. " +
        "Rolling back to the previous consistent state.")]
    public static partial void LogCorruptLuceneIndex(this ILogger logger, string indexName);

    [LoggerMessage(LogLevel.Critical,
        "Failed to commit changes during disposal to '{IndexName}': {ExceptionMessage}")]
    public static partial void LogFailedToCommitDuringDispose(this ILogger logger, string indexName, string exceptionMessage);

    [LoggerMessage(LogLevel.Critical,
        "Failed to commit changes to '{IndexName}' on dispose: {ExceptionMessage}")]
    public static partial void LogFailedToCommitOnDispose(this ILogger logger, string indexName, string exceptionMessage);

    [LoggerMessage(LogLevel.Critical,
        "Failed to index documents in '{IndexName}': {ExceptionMessage}")]
    public static partial void LogFailedToIndexDocuments(this ILogger logger, string indexName, string exceptionMessage);

    [LoggerMessage(LogLevel.Error,
        "Failed to parse document identifier '{DocumentId}'")]
    public static partial void LogFailedToParseDocumentId(this ILogger logger, string documentId);

    [LoggerMessage(LogLevel.Warning,
        "Failed to release searcher for index {IndexName}: {ExceptionMessage}")]
    public static partial void LogFailedToReleaseSearcher(this ILogger logger, string indexName, string exceptionMessage);

    [LoggerMessage(LogLevel.Critical,
        "Failed to rollback in '{IndexName}': {ExceptionMessage}")]
    public static partial void LogFailedToRollback(this ILogger logger, string indexName, string exceptionMessage);

    [LoggerMessage(LogLevel.Critical,
        "Resource '{ResourceName}' could not be located within assembly '{AssemblyName}'")]
    public static partial void LogResourceCouldNotBeLocated(this ILogger logger, string resourceName, string? assemblyName);
}
