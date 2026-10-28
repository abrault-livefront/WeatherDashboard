namespace WeatherDashboard.Infrastructure.Services.Indexer;

/// <summary>
///     Records the hash of the seed data that was last successfully indexed for a given index.
/// </summary>
/// <param name="IndexName">The name of the index the seed hash applies to.</param>
/// <param name="Hash">The hash of the seed data that was indexed.</param>
/// <param name="SeededAtUtc">The time, in UTC, at which the seed completed.</param>
internal sealed record SeedMetadata(string IndexName, string Hash, DateTimeOffset SeededAtUtc);
