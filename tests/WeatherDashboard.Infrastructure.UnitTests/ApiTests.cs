namespace WeatherDashboard.Infrastructure.UnitTests;

using System.Runtime.CompilerServices;
using PublicApiGenerator;
using Serialization.Weather;

[Trait("Category", "ApiContract")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Api")]
[Trait("Speed", "Fast")]
public sealed class ApiTests
{
    [Fact]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task PublicApi_HasNoBreakingChanges_Async()
    {
        string api = typeof(ForecastResponseJsonSerializerContext).Assembly.GeneratePublicApi();

        await Verify(api);
    }
}
