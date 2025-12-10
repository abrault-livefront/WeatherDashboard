namespace WeatherDashboard.Application.UnitTests.Serialization.Json;

using System.Text.Json;
using AwesomeAssertions;
using Common.Serialization.Json;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "Serialization")]
[Trait("Speed", "Fast")]
public sealed class JsonStringTimeZoneInfoConverterTests
{
    private readonly JsonSerializerOptions _options;

    public JsonStringTimeZoneInfoConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new JsonStringTimeZoneInfoConverter());
    }

    [Fact]
    public void Read_WithEmptyString_ShouldReturnNull()
    {
        const string json = "\"\"";

        TimeZoneInfo? result = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Read_WithInvalidTimeZoneId_ShouldReturnNull()
    {
        const string json = "\"Invalid/TimeZone\"";

        TimeZoneInfo? result = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Read_WithNonStringToken_ShouldThrowJsonException()
    {
        const string json = "123";

        Action act = () => JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        act.Should().Throw<JsonException>()
           .WithMessage("Expected string token type for TimeZoneInfo deserialization.");
    }

    [Fact]
    public void Read_WithNullString_ShouldReturnNull()
    {
        const string json = "null";

        TimeZoneInfo? result = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Read_WithObjectToken_ShouldThrowJsonException()
    {
        const string json = "{\"id\":\"America/New_York\"}";

        Action act = () => JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        act.Should().Throw<JsonException>()
           .WithMessage("Expected string token type for TimeZoneInfo deserialization.");
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Australia/Sydney")]
    public void Read_WithValidTimeZoneId_ShouldDeserialize(string timeZoneId)
    {
        string json = $"\"{timeZoneId}\"";

        TimeZoneInfo? result = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        result.Should().NotBeNull();
        result!.Id.Should().Be(timeZoneId);
    }

    [Fact]
    public void Read_WithWhitespaceString_ShouldReturnNull()
    {
        const string json = "\"   \"";

        TimeZoneInfo? result = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        result.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_WithMultipleTimeZones_ShouldPreserveAllValues()
    {
        TimeZoneInfo[] original =
        [
            TimeZoneInfo.Utc,
            TimeZoneInfo.FindSystemTimeZoneById("America/New_York"),
            TimeZoneInfo.FindSystemTimeZoneById("Europe/London"),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"),
        ];

        string json = JsonSerializer.Serialize(original, _options);
        TimeZoneInfo[]? deserialized = JsonSerializer.Deserialize<TimeZoneInfo[]>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized.Should().HaveCount(4);
        for ( int i = 0; i < original.Length; i++ )
        {
            deserialized![i].Id.Should().Be(original[i].Id);
        }
    }

    [Theory]
    [InlineData("America/Chicago")]
    [InlineData("America/Los_Angeles")]
    [InlineData("Pacific/Auckland")]
    public void RoundTrip_WithValidTimeZone_ShouldPreserveValue(string timeZoneId)
    {
        TimeZoneInfo original = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        string json = JsonSerializer.Serialize(original, _options);
        TimeZoneInfo? deserialized = JsonSerializer.Deserialize<TimeZoneInfo>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
    }

    [Fact]
    public void Write_WithUtcTimeZone_ShouldSerialize()
    {
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        string json = JsonSerializer.Serialize(timeZone, _options);

        json.Should().Be("\"UTC\"");
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/Paris")]
    [InlineData("Asia/Dubai")]
    public void Write_WithValidTimeZoneInfo_ShouldSerialize(string timeZoneId)
    {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        string json = JsonSerializer.Serialize(timeZone, _options);

        json.Should().Be($"\"{timeZoneId}\"");
    }
}
