using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities.Payload;

public record MotionReadingPayload(
    [property: JsonPropertyName("motionDetected")] bool IsDetected);