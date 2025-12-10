namespace WeatherDashboard.Domain.Entities.Documents;

/// <summary>
///     Represents a searchable location document containing geographic and postal information.
/// </summary>
/// <param name="Id">The unique identifier for the location.</param>
/// <param name="Locality">The name of the city or locality.</param>
/// <param name="Province">The full name of the province or state.</param>
/// <param name="ProvinceCode">The abbreviated code for the province or state.</param>
/// <param name="PostalCodes">A read-only collection of postal codes associated with this location.</param>
/// <param name="Latitude">The latitude coordinate of the location.</param>
/// <param name="Longitude">The longitude coordinate of the location.</param>
public record LocationDocument(
    Guid Id,
    string Locality,
    string Province,
    string ProvinceCode,
    IReadOnlyList<string> PostalCodes,
    double Latitude,
    double Longitude) : IDocument;
