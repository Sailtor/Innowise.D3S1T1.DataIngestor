using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities.Payload;

public class MotionReadingPayload
{
    [JsonPropertyName("motionDetected")]
    public bool IsDetected { get; set; }
}