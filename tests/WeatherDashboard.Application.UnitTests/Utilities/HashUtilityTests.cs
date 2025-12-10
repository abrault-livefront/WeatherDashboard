namespace WeatherDashboard.Application.UnitTests.Utilities;

using AutoFixture;
using AwesomeAssertions;
using Common.Utilities;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "Caching")]
[Trait("Speed", "Fast")]
public sealed class HashUtilityTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void HashString_WithDifferentInputs_ReturnsDifferentHashes()
    {
        string input1 = _fixture.Create<string>();
        string input2 = _fixture.Create<string>();

        string hash1 = HashUtility.HashString(input1);
        string hash2 = HashUtility.HashString(input2);

        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData(" \t\n")]
    public void HashString_WithInvalidInput_ThrowsArgumentException(string input)
    {
        Action act = () => HashUtility.HashString(input);

        act.Should().Throw<ArgumentException>()
           .WithParameterName(nameof(input));
    }

    [Fact]
    public void HashString_WithKnownInput_ReturnsExpectedHash()
    {
        const string input = "Hello, World!";
        const string expectedHash = "DFFD6021BB2BD5B0AF676290809EC3A53191DD81C7F70A4B28688A362182986F";

        string actualHash = HashUtility.HashString(input);

        actualHash.Should().Be(expectedHash);
    }

    [Fact]
    public void HashString_WithNullInput_ThrowsArgumentNullException()
    {
        Action act = () => HashUtility.HashString(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("Test String")]
    [InlineData("!@#$%^&*()_+-=[]{}|;':\",.<>/?`~")]
    public void HashString_WithValidInput_ReturnsConsistentHash(string input)
    {
        string result = HashUtility.HashString(input);

        result.Should().NotBeNullOrWhiteSpace();
        result.Should().MatchRegex("^[A-F0-9]+$");
        result.Length.Should().Be(64); // SHA-256 produces a 64-character hex string
    }
}
