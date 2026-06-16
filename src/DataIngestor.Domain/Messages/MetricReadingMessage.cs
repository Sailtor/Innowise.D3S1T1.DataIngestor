using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Messages;

public record MetricReadingMessage(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("payload")] object Payload);
