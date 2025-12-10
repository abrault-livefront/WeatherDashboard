namespace WeatherDashboard.Application.UnitTests.Serialization.Json;

using AwesomeAssertions;
using Common.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;

[Trait("Category", "Unit")]
[Trait("Layer", "Application")]
[Trait("Feature", "Serialization")]
[Trait("Speed", "Fast")]
public sealed class TimeZoneInfoFormatterTests
{
    private readonly MessagePackSerializerOptions _options = MessagePackSerializerOptions.Standard.WithResolver(
        CompositeResolver.Create(
            [new TimeZoneInfoFormatter(),],
            [StandardResolver.Instance,]
        )
    );

    [Fact]
    public void Deserialize_WithEmptyString_ShouldReturnNull()
    {
        byte[] data = MessagePackSerializer.Serialize(string.Empty, _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithInvalidTimeZoneId_ShouldReturnNull()
    {
        byte[] data = MessagePackSerializer.Serialize("Invalid/TimeZone", _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithNil_ShouldReturnNull()
    {
        byte[] data = MessagePackSerializer.Serialize<string?>(null, _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Australia/Sydney")]
    public void Deserialize_WithValidTimeZoneId_ShouldDeserialize(string timeZoneId)
    {
        byte[] data = MessagePackSerializer.Serialize(timeZoneId, _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Id.Should().Be(timeZoneId);
    }

    [Fact]
    public void Deserialize_WithWhitespaceString_ShouldReturnNull()
    {
        byte[] data = MessagePackSerializer.Serialize("   ", _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_WithMultipleTimeZones_ShouldPreserveAllValues()
    {
        TimeZoneInfo?[] original =
        [
            TimeZoneInfo.Utc,
            TimeZoneInfo.FindSystemTimeZoneById("America/New_York"),
            TimeZoneInfo.FindSystemTimeZoneById("Europe/London"),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"),
            null,
        ];

        byte[] data = MessagePackSerializer.Serialize(original, _options, TestContext.Current.CancellationToken);
        TimeZoneInfo?[] deserialized = MessagePackSerializer.Deserialize<TimeZoneInfo?[]>(data, _options, TestContext.Current.CancellationToken);

        deserialized.Should().NotBeNull();
        deserialized.Should().HaveCount(5);
        for ( int i = 0; i < original.Length - 1; i++ )
        {
            deserialized[i].Should().NotBeNull();
            deserialized[i]!.Id.Should().Be(original[i]!.Id);
        }

        deserialized[4].Should().BeNull();
    }

    [Theory]
    [InlineData("America/Chicago")]
    [InlineData("America/Los_Angeles")]
    [InlineData("Pacific/Auckland")]
    public void RoundTrip_WithValidTimeZone_ShouldPreserveValue(string timeZoneId)
    {
        TimeZoneInfo original = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        byte[] data = MessagePackSerializer.Serialize(original, _options, TestContext.Current.CancellationToken);
        TimeZoneInfo? deserialized = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        deserialized.Should().NotBeNull();
        deserialized.Id.Should().Be(original.Id);
    }

    [Fact]
    public void Serialize_WithNull_ShouldWriteNil()
    {
        byte[] data = MessagePackSerializer.Serialize<TimeZoneInfo?>(null, _options, TestContext.Current.CancellationToken);

        TimeZoneInfo? result = MessagePackSerializer.Deserialize<TimeZoneInfo?>(data, _options, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public void Serialize_WithUtcTimeZone_ShouldSerialize()
    {
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        byte[] data = MessagePackSerializer.Serialize(timeZone, _options, TestContext.Current.CancellationToken);
        string result = MessagePackSerializer.Deserialize<string>(data, _options, TestContext.Current.CancellationToken);

        result.Should().Be("UTC");
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/Paris")]
    [InlineData("Asia/Dubai")]
    public void Serialize_WithValidTimeZoneInfo_ShouldSerialize(string timeZoneId)
    {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        byte[] data = MessagePackSerializer.Serialize(timeZone, _options, TestContext.Current.CancellationToken);
        string result = MessagePackSerializer.Deserialize<string>(data, _options, TestContext.Current.CancellationToken);

        result.Should().Be(timeZoneId);
    }
}
