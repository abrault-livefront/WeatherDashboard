namespace WeatherDashboard.Infrastructure.Providers;

using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;

[ExcludeFromCodeCoverage]
internal sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
