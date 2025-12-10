namespace WeatherDashboard.Web.Configuration;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Configuration settings for the default location used in the weather dashboard.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Configuration settings are intended to be public for DI binding.")]
public sealed class DefaultLocationSettings
{
    /// <summary>
    ///     The configuration section name used to bind these settings from appsettings.json.
    /// </summary>
    public const string SectionName = "DefaultLocation";

    /// <summary>
    ///     Gets or initializes the latitude coordinate of the default location.
    /// </summary>
    /// <value>The latitude in decimal degrees. Default is 25.77427 (Miami, FL).</value>
    public double Latitude { get; init; } = 25.77427;

    /// <summary>
    ///     Gets or initializes the locality (city) name of the default location.
    /// </summary>
    /// <value>The city name. Default is "Miami".</value>
    public string Locality { get; init; } = "Miami";

    /// <summary>
    ///     Gets or initializes the longitude coordinate of the default location.
    /// </summary>
    /// <value>The longitude in decimal degrees. Default is -80.19366 (Miami, FL).</value>
    public double Longitude { get; init; } = -80.19366;

    /// <summary>
    ///     Gets or initializes the province (state) name of the default location.
    /// </summary>
    /// <value>The state name. Default is "Florida".</value>
    public string Province { get; init; } = "Florida";

    /// <summary>
    ///     Gets or initializes the province (state) code of the default location.
    /// </summary>
    /// <value>The two-letter state code. Default is "FL".</value>
    public string ProvinceCode { get; init; } = "FL";
}
