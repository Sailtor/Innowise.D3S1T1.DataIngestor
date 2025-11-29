using DataIngestor.Domain.Entities;

namespace DataIngestor.Application.Interfaces;

public interface IMetricReader
{
    Task<List<MetricReadingBase>> ReadMetricsAsync(CancellationToken cancellationToken = default);
}
