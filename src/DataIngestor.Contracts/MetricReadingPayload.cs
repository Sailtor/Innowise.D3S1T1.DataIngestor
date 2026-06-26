using System.Text.Json.Serialization;

namespace DataIngestor.Contracts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnergyReadingPayload), typeDiscriminator: "energy")]
[JsonDerivedType(typeof(AirQualityReadingPayload), typeDiscriminator: "air_quality")]
[JsonDerivedType(typeof(MotionReadingPayload), typeDiscriminator: "motion")]
public abstract record MetricReadingPayload;
