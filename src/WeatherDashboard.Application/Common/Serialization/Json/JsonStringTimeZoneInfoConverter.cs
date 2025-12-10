namespace WeatherDashboard.Application.Common.Serialization.Json;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///     Provides JSON serialization and deserialization for <see cref="TimeZoneInfo" /> objects.
/// </summary>
public sealed class JsonStringTimeZoneInfoConverter : JsonConverter<TimeZoneInfo>
{
    /// <summary>
    ///     Deserializes a <see cref="TimeZoneInfo" /> object from JSON format.
    /// </summary>
    /// <param name="reader">The JSON reader to read from.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>
    ///     The deserialized <see cref="TimeZoneInfo" /> object if the time zone ID is valid; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="JsonException">
    ///     Thrown when the JSON token is not a string.
    /// </exception>
    public override TimeZoneInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if ( reader.TokenType is not JsonTokenType.String )
        {
            throw new JsonException("Expected string token type for TimeZoneInfo deserialization.");
        }

        string? valueAsString = reader.GetString();
        if ( string.IsNullOrWhiteSpace(valueAsString) )
        {
            return null;
        }

        return TimeZoneInfo.TryFindSystemTimeZoneById(valueAsString, out TimeZoneInfo? timeZoneInfo)
                   ? timeZoneInfo
                   : null;
    }

    /// <summary>
    ///     Serializes a <see cref="TimeZoneInfo" /> object to JSON format.
    /// </summary>
    /// <param name="writer">The JSON writer to write to.</param>
    /// <param name="value">The <see cref="TimeZoneInfo" /> object to serialize.</param>
    /// <param name="options">The serializer options.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="writer" /> or <paramref name="value" /> is <c>null</c>.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, TimeZoneInfo value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStringValue(value.Id);
    }
}
