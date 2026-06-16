using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities.Payload;

public record EnergyReadingPayload(
    [property: JsonPropertyName("energy")] double Amount);
