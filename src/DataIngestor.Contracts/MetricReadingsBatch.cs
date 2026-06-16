namespace DataIngestor.Contracts;

public record MetricReadingsBatch(
    IReadOnlyList<MetricReadingMessage> Readings,
    DateTime IngestedAtUtc);
