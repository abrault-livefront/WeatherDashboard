namespace WeatherDashboard.Domain.UnitTests;

using System.Runtime.CompilerServices;
using PublicApiGenerator;
using Serialization.Json;

[Trait("Category", "ApiContract")]
[Trait("Layer", "Domain")]
[Trait("Feature", "Api")]
[Trait("Speed", "Fast")]
public sealed class ApiTests
{
    [Fact]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task PublicApi_HasNoBreakingChanges_Async()
    {
        string api = typeof(DocumentJsonSerializerContext).Assembly.GeneratePublicApi();

        await Verify(api);
    }
}
