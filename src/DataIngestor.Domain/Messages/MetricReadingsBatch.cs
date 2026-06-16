namespace DataIngestor.Domain.Messages;

public record MetricReadingsBatch(
    IReadOnlyList<MetricReadingMessage> Readings,
    DateTime IngestedAt);
