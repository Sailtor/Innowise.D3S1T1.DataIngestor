namespace DataIngestor.Application.Interfaces;

public interface IMetricsIngestionService
{
    Task IngestAsync(CancellationToken cancellationToken = default);
}
