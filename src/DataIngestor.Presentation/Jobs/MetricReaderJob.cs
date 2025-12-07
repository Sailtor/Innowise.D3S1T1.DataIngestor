using DataIngestor.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DataIngestor.Presentation.Jobs;

public class MetricReaderJob(
    IMetricReader metricReader,
    ILogger<MetricReaderJob> logger) : IJob
{
    private readonly IMetricReader metricReader = metricReader;
    private readonly ILogger<MetricReaderJob> logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation($"Metrics fetch started at {DateTime.UtcNow}");
        var metrics = await metricReader.ReadMetricsAsync();
        logger.LogInformation(metrics.ToString()); // To test metrics reading. Will be switched to MQ message posting
    }
}