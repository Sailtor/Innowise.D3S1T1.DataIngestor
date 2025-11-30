// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using DataIngestor.Domain.Entities.Payload;
using DataIngestor.Domain.ValueObjects;

namespace DataIngestor.Domain.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MetricReading<EnergyReadingPayload>), typeDiscriminator: "energy")]
[JsonDerivedType(typeof(MetricReading<AirQualityReadingPayload>), typeDiscriminator: "air_quality")]
[JsonDerivedType(typeof(MetricReading<MotionReadingPayload>), typeDiscriminator: "motion")]
public abstract class MetricReadingBase
{
    [JsonPropertyName("name")]
    public required Location Location { get; set; }
}
