using AutoMapper;
using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Messages;
using MassTransit;

namespace DataIngestor.Infrastructure.Messaging;

public class MetricPublisher(IPublishEndpoint publishEndpoint, IMapper mapper) : IMetricPublisher
{
    private readonly IPublishEndpoint publishEndpoint = publishEndpoint;
    private readonly IMapper mapper = mapper;

    public async Task PublishAsync(IReadOnlyList<MetricReadingBase> readings, CancellationToken cancellationToken = default)
    {
        var messages = mapper.Map<List<MetricReadingMessage>>(readings);
        var batch = new MetricReadingsBatch(messages, DateTime.UtcNow);
        await publishEndpoint.Publish(batch, cancellationToken);
    }
}
