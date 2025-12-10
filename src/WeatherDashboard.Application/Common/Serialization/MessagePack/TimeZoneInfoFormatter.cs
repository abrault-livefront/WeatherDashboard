namespace WeatherDashboard.Application.Common.Serialization.MessagePack;

using global::MessagePack;
using global::MessagePack.Formatters;

/// <summary>
///     Provides MessagePack serialization and deserialization for <see cref="TimeZoneInfo" /> objects.
/// </summary>
public sealed class TimeZoneInfoFormatter : IMessagePackFormatter<TimeZoneInfo?>
{
    /// <summary>
    ///     Deserializes a <see cref="TimeZoneInfo" /> object from MessagePack format.
    /// </summary>
    /// <param name="reader">The MessagePack reader to read from.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>
    ///     The deserialized <see cref="TimeZoneInfo" /> object if the time zone ID is valid; otherwise, <c>null</c>.
    /// </returns>
    public TimeZoneInfo? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if ( reader.TryReadNil() )
        {
            return null;
        }

        string? valueAsString = reader.ReadString();
        if ( string.IsNullOrWhiteSpace(valueAsString) )
        {
            return null;
        }


        return TimeZoneInfo.TryFindSystemTimeZoneById(valueAsString, out TimeZoneInfo? timeZoneInfo)
                   ? timeZoneInfo
                   : null;
    }

    /// <summary>
    ///     Serializes a <see cref="TimeZoneInfo" /> object to MessagePack format.
    /// </summary>
    /// <param name="writer">The MessagePack writer to write to.</param>
    /// <param name="value">The <see cref="TimeZoneInfo" /> object to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public void Serialize(ref MessagePackWriter writer, TimeZoneInfo? value, MessagePackSerializerOptions options)
    {
        if ( value is null )
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.Id);
    }
}
