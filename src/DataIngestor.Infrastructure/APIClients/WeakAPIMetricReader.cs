using System.Net.Http.Json;
using System.Text.Json;
using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Entities;
using DataIngestor.Infrastructure.Converters;
using Microsoft.Extensions.Logging;

namespace DataIngestor.Infrastructure.APIClients;

public class WeakAPIMetricReader(
    HttpClient httpClient,
    ILogger<WeakAPIMetricReader> logger) : IMetricReader
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new LocationJsonConverter() },
    };

    public async Task<List<MetricReadingBase>> ReadMetricsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Metrics fetch started");
        List<MetricReadingBase>? readings = await httpClient.GetFromJsonAsync<List<MetricReadingBase>>("meters", jsonOptions, cancellationToken);
        logger.LogInformation("Metrics fetch finished");

        return readings ?? [];
    }
}
