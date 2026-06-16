using System.Text.Json;
using System.Text.Json.Nodes;
using AutoMapper;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Messages;
using DataIngestor.Infrastructure.Converters;

namespace DataIngestor.Infrastructure.Messaging;

public class MetricReadingProfile : Profile
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new LocationJsonConverter() }
    };

    public MetricReadingProfile()
    {
        CreateMap<MetricReadingBase, MetricReadingMessage>()
            .ForCtorParam("Type", opt => opt.MapFrom(src => GetType(src)))
            .ForCtorParam("Room", opt => opt.MapFrom(src => src.Location.Name))
            .ForCtorParam("Payload", opt => opt.MapFrom(src => GetPayload(src)));
    }

    private static string GetType(MetricReadingBase src)
    {
        var node = JsonSerializer.SerializeToNode(src, SerializerOptions);
        return node?.AsObject()["type"]?.GetValue<string>() ?? "unknown";
    }

    private static object GetPayload(MetricReadingBase src)
    {
        var node = JsonSerializer.SerializeToNode(src, SerializerOptions);
        return (object)(node?.AsObject()["payload"] ?? new JsonObject());
    }
}
