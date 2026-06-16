using DataIngestor.Domain.Entities;

namespace DataIngestor.Application.Interfaces;

public interface IMetricPublisher
{
    Task PublishAsync(IReadOnlyList<MetricReadingBase> readings, CancellationToken cancellationToken = default);
}
