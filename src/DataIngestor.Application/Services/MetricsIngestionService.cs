using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DataIngestor.Application.Services;

public class MetricsIngestionService(
    IMetricReader metricReader,
    IMetricPublisher metricPublisher,
    ILogger<MetricsIngestionService> logger) : IMetricsIngestionService
{
    public async Task IngestAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Metrics ingestion started at {Time}", DateTime.UtcNow);

        List<MetricReadingBase> metrics = await metricReader.ReadMetricsAsync(cancellationToken);
        logger.LogInformation("Fetched {Count} metrics", metrics.Count);

        await metricPublisher.PublishAsync(metrics, cancellationToken);
        logger.LogInformation("Published batch of {Count} metrics to message queue", metrics.Count);
    }
}
