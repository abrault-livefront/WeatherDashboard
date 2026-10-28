namespace WeatherDashboard.Infrastructure.UnitTests.Services.Search;

using AwesomeAssertions;
using Infrastructure.Services.Search;

[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Feature", "Search")]
[Trait("Speed", "Fast")]
public sealed class LocationSearchQuerySanitizerTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("a", false)]
    [InlineData("ab", false)]
    [InlineData("abc", true)]
    [InlineData("  ab  ", false)]
    [InlineData("**", false)]
    public void TrySanitize_EnforcesMinimumLength(string query, bool expected)
    {
        bool result = LocationSearchQuerySanitizer.TrySanitize(query, out _);

        result.Should().Be(expected);
    }

    [Fact]
    public void TrySanitize_StripsWildcardCharacters()
    {
        LocationSearchQuerySanitizer.TrySanitize("aus*t?n", out string sanitized);

        sanitized.Should().Be("austn");
    }

    [Fact]
    public void TrySanitize_TrimsWhitespace()
    {
        LocationSearchQuerySanitizer.TrySanitize("  austin  ", out string sanitized);

        sanitized.Should().Be("austin");
    }

    [Fact]
    public void TrySanitize_LowercasesQuery()
    {
        LocationSearchQuerySanitizer.TrySanitize("AUSTIN", out string sanitized);

        sanitized.Should().Be("austin");
    }
}
