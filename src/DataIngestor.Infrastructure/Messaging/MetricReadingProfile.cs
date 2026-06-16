using AutoMapper;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Entities.Payload;
using ContractAirQuality = DataIngestor.Contracts.AirQualityReadingPayload;
using ContractEnergy = DataIngestor.Contracts.EnergyReadingPayload;
using ContractMessage = DataIngestor.Contracts.MetricReadingMessage;
using ContractMotion = DataIngestor.Contracts.MotionReadingPayload;
using ContractPayload = DataIngestor.Contracts.MetricReadingPayload;

namespace DataIngestor.Infrastructure.Messaging;

public class MetricReadingProfile : Profile
{
    public MetricReadingProfile()
    {
        CreateMap<EnergyReadingPayload, ContractEnergy>()
            .ConstructUsing(src => new ContractEnergy(src.Amount));

        CreateMap<MotionReadingPayload, ContractMotion>()
            .ConstructUsing(src => new ContractMotion(src.IsDetected));

        CreateMap<AirQualityReadingPayload, ContractAirQuality>()
            .ConstructUsing(src => new ContractAirQuality(src.Co2, src.Pm25, src.Humidity));

        CreateMap<MetricReadingBase, ContractMessage>()
            .ForCtorParam(nameof(ContractMessage.Room), opt => opt.MapFrom(src => src.Location.Name))
            .ForCtorParam(nameof(ContractMessage.Payload), opt => opt.MapFrom((src, ctx) => MapPayload(src, ctx.Mapper)));
    }

    private static ContractPayload MapPayload(MetricReadingBase src, IRuntimeMapper mapper) => src switch
    {
        MetricReading<EnergyReadingPayload> r => mapper.Map<ContractEnergy>(r.Payload),
        MetricReading<MotionReadingPayload> r => mapper.Map<ContractMotion>(r.Payload),
        MetricReading<AirQualityReadingPayload> r => mapper.Map<ContractAirQuality>(r.Payload),
        _ => throw new InvalidOperationException($"Unknown metric reading type: {src.GetType().Name}"),
    };
}
