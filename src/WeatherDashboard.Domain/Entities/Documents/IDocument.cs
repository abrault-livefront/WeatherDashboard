namespace WeatherDashboard.Domain.Entities.Documents;

/// <summary>
///     Interface representing a document entity with a unique identifier.
/// </summary>
public interface IDocument
{
    /// <summary>
    ///     The unique identifier of the document.
    /// </summary>
    Guid Id { get; }
}
