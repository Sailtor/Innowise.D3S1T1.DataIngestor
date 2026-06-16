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
            var metrics = await metricReader.ReadMetricsAsync();
            logger.LogInformation("Fetched {Count} metrics: {Payload}", metrics.Count, JsonSerializer.Serialize(metrics)); // To test metrics reading. Will be switched to MQ message posting
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Metrics fetch failed. Will retry on next trigger.");
        }
    }
}