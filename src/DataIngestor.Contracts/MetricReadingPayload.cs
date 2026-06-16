using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnergyReadingPayload), typeDiscriminator: "energy")]
[JsonDerivedType(typeof(AirQualityReadingPayload), typeDiscriminator: "air_quality")]
[JsonDerivedType(typeof(MotionReadingPayload), typeDiscriminator: "motion")]
public abstract record MetricReadingPayload;

public record EnergyReadingPayload(
    [property: JsonPropertyName("energy")] double Amount) : MetricReadingPayload;

public record AirQualityReadingPayload(
    [property: JsonPropertyName("co2")] double Co2,
    [property: JsonPropertyName("pm25")] double Pm25,
    [property: JsonPropertyName("humidity")] double Humidity) : MetricReadingPayload;

public record MotionReadingPayload(
    [property: JsonPropertyName("motionDetected")] bool IsDetected) : MetricReadingPayload;
