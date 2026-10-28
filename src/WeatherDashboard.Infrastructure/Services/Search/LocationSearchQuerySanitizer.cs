namespace WeatherDashboard.Infrastructure.Services.Search;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Sanitizes raw location search query strings prior to executing them against Elasticsearch.
/// </summary>
internal static class LocationSearchQuerySanitizer
{
    private const int s_minimumQueryLength = 3;

    /// <summary>
    ///     Strips wildcard characters, trims, and lowercases <paramref name="query" />.
    /// </summary>
    /// <param name="query">The raw search query string provided by the user.</param>
    /// <param name="sanitized">The sanitized query string.</param>
    /// <returns>
    ///     <c>true</c> if the sanitized query meets the minimum length requirement; otherwise, <c>false</c>.
    /// </returns>
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
    public static bool TrySanitize(string query, out string sanitized)
    {
        sanitized = query.Replace("*", string.Empty, StringComparison.OrdinalIgnoreCase)
                          .Replace("?", string.Empty, StringComparison.OrdinalIgnoreCase)
                          .Trim()
                          .ToLowerInvariant();

        return sanitized.Length >= s_minimumQueryLength;
    }
}
