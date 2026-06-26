namespace DataIngestor.Infrastructure.Options;

public class WeakApiOptions
{
    public const string SectionName = "Integrations:WeakAPI";
    public const string ResilienceSectionName = "Integrations:WeakAPI:Resilience";

    public required Uri Url { get; init; }

    public required string ApiKey { get; init; }
}
