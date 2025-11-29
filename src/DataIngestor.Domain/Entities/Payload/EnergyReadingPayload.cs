using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities.Payload;

public class EnergyReadingPayload
{
    [JsonPropertyName("energy")]
    public double Amount { get; set; }
}
