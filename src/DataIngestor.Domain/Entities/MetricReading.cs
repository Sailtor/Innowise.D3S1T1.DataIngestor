using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities;

public class MetricReading<T> : MetricReadingBase
{
    [JsonPropertyName("payload")]
    public T Payload { get; set; } = default!;
}
