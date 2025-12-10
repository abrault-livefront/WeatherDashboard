namespace WeatherDashboard.Web.Features.Common;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Code-behind for the Error component that displays error information with request tracking.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
public partial class Error : ComponentBase
{
    /// <summary>
    ///     Gets a value indicating whether the request ID should be shown to the user.
    /// </summary>
    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    ///     Gets or sets the current HTTP context from the cascading parameter.
    /// </summary>
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>
    ///     Gets or sets the request ID used for tracking the error.
    /// </summary>
    private string? RequestId { get; set; }

    /// <summary>
    ///     Initializes the component by setting the request ID from the current activity or HTTP context.
    /// </summary>
    protected override void OnInitialized()
    {
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }
}
