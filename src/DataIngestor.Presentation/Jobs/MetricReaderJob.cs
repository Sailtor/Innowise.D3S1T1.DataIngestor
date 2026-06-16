using System.Text.Json;
using DataIngestor.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DataIngestor.Presentation.Jobs;

public class MetricReaderJob(
    IServiceScopeFactory scopeFactory,
    ILogger<MetricReaderJob> logger) : IJob
{
    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<MetricReaderJob> logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var metricReader = scope.ServiceProvider.GetRequiredService<IMetricReader>();

        logger.LogInformation($"Metrics fetch started at {DateTime.UtcNow}");
        try
        {
            var metricPublisher = scope.ServiceProvider.GetRequiredService<IMetricPublisher>();

            var metrics = await metricReader.ReadMetricsAsync();
            logger.LogInformation("Fetched {Count} metrics: {Payload}", metrics.Count, JsonSerializer.Serialize(metrics));

            await metricPublisher.PublishAsync(metrics);
            logger.LogInformation("Published batch of {Count} metrics to message queue", metrics.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Metrics fetch/publish failed. Will retry on next trigger.");
        }
    }
}