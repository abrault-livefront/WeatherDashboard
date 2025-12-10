namespace WeatherDashboard.Domain.Serialization.Json;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Entities.Documents;

/// <summary>
///     Provides JSON serialization context for document types using source generation.
/// </summary>
/// <remarks>
///     This context is configured with web defaults and includes serialization support for
///     <see cref="Entities.Documents.LocationDocument" /> and collections of location documents. Using source
///     generation improves performance and reduces runtime overhead for JSON serialization.
/// </remarks>
[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(LocationDocument))]
[JsonSerializable(typeof(List<LocationDocument>))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
public sealed partial class DocumentJsonSerializerContext : JsonSerializerContext;
