using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

public record MetricReadingMessage(
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("payload")] MetricReadingPayload Payload);
