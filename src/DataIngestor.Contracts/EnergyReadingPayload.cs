using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

public record EnergyReadingPayload(
    [property: JsonPropertyName("energy")] double Amount) : MetricReadingPayload;
