namespace WeatherDashboard.Web.Features.Weather.Components;

/// <summary>
///     Code-behind for the WeatherMetricDisplay component that displays a labeled weather metric.
/// </summary>
public partial class WeatherMetricDisplay : ComponentBase
{
    /// <summary>
    ///     Gets or sets the child content to render inside the metric display (typically the metric value).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    ///     Gets or sets the label text describing the weather metric.
    /// </summary>
    [Parameter]
    public string Label { get; set; } = null!;
}
