using System.Text.Json.Serialization;

namespace DataIngestor.Domain.Entities.Payload;

internal class AirQualityReadingPayload
{
    [JsonPropertyName("co2")]
    public double Co2 { get; set; }

    [JsonPropertyName("pm25")]
    public double Pm25 { get; set; }

    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }
}
