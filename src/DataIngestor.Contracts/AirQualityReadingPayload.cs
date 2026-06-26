using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

public record AirQualityReadingPayload(
    [property: JsonPropertyName("co2")] double Co2,
    [property: JsonPropertyName("pm25")] double Pm25,
    [property: JsonPropertyName("humidity")] double Humidity) : MetricReadingPayload;
