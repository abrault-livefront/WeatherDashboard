namespace WeatherDashboard.Web.Configuration;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Provides configuration settings for local storage, including naming conventions
///     and environment-specific isolation.
/// </summary>
[SuppressMessage("Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Configuration settings are intended to be public for DI binding.")]
public sealed class LocalStorageSettings
{
    /// <summary>
    ///     The configuration section name used to bind these settings from appsettings.json.
    /// </summary>
    public const string SectionName = "LocalStorageSettings";

    /// <summary>
    ///     Gets or initializes a value indicating whether to include the environment name in local storage keys
    ///     for environment-specific isolation.
    ///     Default is <see langword="true" />.
    /// </summary>
    public bool IncludeEnvironmentInName { get; init; } = true;

    /// <summary>
    ///     Gets or initializes the prefix to prepend to all local storage keys.
    ///     Default is "WeatherDashboard.Web".
    /// </summary>
    public string Prefix { get; init; } = "WeatherDashboard.Web";

    /// <summary>
    ///     Generates a formatted local storage key by combining the prefix, key, and optionally the environment name.
    /// </summary>
    /// <param name="key">The base key name for the local storage item.</param>
    /// <param name="environment">
    ///     The environment name to include in the key if <see cref="IncludeEnvironmentInName" /> is
    ///     <see langword="true" />. Optional.
    /// </param>
    /// <returns>
    ///     A formatted local storage key in the format "{Prefix}.{key}" or "{Prefix}.{key}.{environment}"
    ///     if an environment is provided and <see cref="IncludeEnvironmentInName" /> is <see langword="true" />.
    /// </returns>
    public string GetLocalStorageKey(string key, string? environment = null)
    {
        if ( IncludeEnvironmentInName && !string.IsNullOrWhiteSpace(environment) )
        {
            return $"{Prefix}.{key}.{environment}";
        }

        return $"{Prefix}.{key}";
    }
}
