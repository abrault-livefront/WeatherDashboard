namespace WeatherDashboard.Web.Configuration;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Provides configuration settings for application cookies, including naming conventions
///     and environment-specific isolation.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Configuration settings are intended to be public for DI binding.")]
public sealed class CookieSettings
{
    /// <summary>
    ///     The configuration section name used to bind these settings from appsettings.json.
    /// </summary>
    public const string SectionName = "CookieSettings";

    /// <summary>
    ///     Gets or initializes the base cookie names.
    ///     Default is a new instance of <see cref="CookieNames" />.
    /// </summary>
    public CookieNames Cookies { get; init; } = new();

    /// <summary>
    ///     Gets or initializes a value indicating whether to include the environment name in cookie names
    ///     for environment-specific isolation.
    ///     Default is <see langword="true" />.
    /// </summary>
    public bool IncludeEnvironmentInName { get; init; } = true;

    /// <summary>
    ///     Gets or initializes the prefix to prepend to all cookie names.
    ///     Default is "WeatherDashboard.Web".
    /// </summary>
    public string Prefix { get; init; } = "WeatherDashboard.Web";

    /// <summary>
    ///     Gets the fully qualified anti-forgery cookie name, optionally including the environment name.
    /// </summary>
    /// <param name="environment">
    ///     The environment name to include in the cookie name (e.g., "Development", "Production").
    ///     If <see langword="null" /> or <see cref="IncludeEnvironmentInName" /> is <see langword="false" />,
    ///     the environment name is not included.
    /// </param>
    /// <returns>The fully qualified anti-forgery cookie name.</returns>
    public string GetAntiForgeryCookieName(string? environment = null)
    {
        return GetCookieName(Cookies.AntiForgery, environment);
    }

    /// <summary>
    ///     Gets a fully qualified cookie name by combining the prefix, cookie type, and optionally the environment name.
    /// </summary>
    /// <param name="cookieType">The base cookie type name (e.g., "Culture", "AntiForgery").</param>
    /// <param name="environment">
    ///     The environment name to include in the cookie name (e.g., "Development", "Production").
    ///     If <see langword="null" /> or <see cref="IncludeEnvironmentInName" /> is <see langword="false" />,
    ///     the environment name is not included.
    /// </param>
    /// <returns>
    ///     A fully qualified cookie name in the format: Prefix.CookieType.Environment
    ///     (if environment is included), or Prefix.CookieType (if not included).
    /// </returns>
    public string GetCookieName(string cookieType, string? environment = null)
    {
        List<string> parts =
        [
            Prefix,
            cookieType,
        ];

        if ( IncludeEnvironmentInName && !string.IsNullOrWhiteSpace(environment) )
        {
            parts.Add(environment);
        }

        return string.Join('.', parts);
    }

    /// <summary>
    ///     Gets the fully qualified culture cookie name, optionally including the environment name.
    /// </summary>
    /// <param name="environment">
    ///     The environment name to include in the cookie name (e.g., "Development", "Production").
    ///     If <see langword="null" /> or <see cref="IncludeEnvironmentInName" /> is <see langword="false" />,
    ///     the environment name is not included.
    /// </param>
    /// <returns>The fully qualified culture cookie name.</returns>
    public string GetCultureCookieName(string? environment = null)
    {
        return GetCookieName(Cookies.Culture, environment);
    }
}
