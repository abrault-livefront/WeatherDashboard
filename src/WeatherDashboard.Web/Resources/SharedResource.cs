#pragma warning disable IDE0130

// ReSharper disable CheckNamespace

// The namespace intentionally does not match the file path to comply with ASP.NET Core localization conventions.
// Resource files (.resx) must be in the same namespace as the marker class they reference.
// By placing this marker class in the root namespace (WeatherDashboard.Web), resource files can be
// organized in the Localizations folder while maintaining proper namespace alignment for IStringLocalizer<T>.

namespace WeatherDashboard.Web;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Localization;

/// <summary>
///     Marker class for shared localization resources in the ASP.NET Core application.
/// </summary>
/// <remarks>
///     This class serves as a marker type for <see cref="IStringLocalizer{T}" /> to locate
///     and load localization resource files (.resx) from the Localizations folder.
///     The class itself is never instantiated; it exists purely to satisfy the generic
///     type constraint of the string localizer infrastructure.
/// </remarks>
[SuppressMessage("ReSharper",
    "ClassNeverInstantiated.Global",
    Justification = "Marker Class")]
[SuppressMessage("Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Marker Class")]
public class SharedResource;
