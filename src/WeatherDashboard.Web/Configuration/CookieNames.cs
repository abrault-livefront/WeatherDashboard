namespace WeatherDashboard.Web.Configuration;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Defines the base names for cookies used by the application.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Configuration settings are intended to be public for DI binding.")]
public sealed class CookieNames
{
    /// <summary>
    ///     Gets or initializes the base name for the anti-forgery cookie.
    ///     Default is "AntiForgery".
    /// </summary>
    public string AntiForgery { get; init; } = "AntiForgery";

    /// <summary>
    ///     Gets or initializes the base name for the culture cookie used for localization.
    ///     Default is "Culture".
    /// </summary>
    public string Culture { get; init; } = "Culture";
}
