namespace WeatherDashboard.Application.UnitTests;

using System.Runtime.CompilerServices;
using Common.Interfaces;
using PublicApiGenerator;

[Trait("Category", "ApiContract")]
[Trait("Layer", "Application")]
[Trait("Feature", "Api")]
[Trait("Speed", "Fast")]
public sealed class ApiTests
{
    [Fact]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task PublicApi_HasNoBreakingChanges_Async()
    {
        string api = typeof(IWeatherService).Assembly.GeneratePublicApi();

        await Verify(api);
    }
}
