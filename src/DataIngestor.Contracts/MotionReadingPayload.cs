using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

public record MotionReadingPayload(
    [property: JsonPropertyName("motionDetected")] bool IsDetected) : MetricReadingPayload;
