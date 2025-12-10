namespace WeatherDashboard.Web.Features.Common;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Code-behind for the LoadingOverlay component that displays a loading indicator overlay.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class LoadingOverlay : ComponentBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the loading overlay should be displayed.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }
}
