namespace WeatherDashboard.Application.Common.Interfaces;

/// <summary>
///     Provides access to the current time.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    ///     Gets the current date and time with offset information.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTimeOffset" /> representing the current date and time.
    /// </value>
    DateTimeOffset Now { get; }
}
