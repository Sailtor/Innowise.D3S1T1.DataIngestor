using DataIngestor.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DataIngestor.Infrastructure.Jobs;

public class MetricReaderJob(
    IServiceScopeFactory scopeFactory,
    ILogger<MetricReaderJob> logger) : IJob
{
    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<MetricReaderJob> logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IMetricsIngestionService ingestionService = scope.ServiceProvider.GetRequiredService<IMetricsIngestionService>();

        try
        {
            await ingestionService.IngestAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Metrics ingestion failed. Will retry on next trigger.");
        }
    }
}
