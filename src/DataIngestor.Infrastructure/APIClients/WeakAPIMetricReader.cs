using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Entities;
using DataIngestor.Infrastructure.Converters;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace DataIngestor.Infrastructure.APIClients;

public class WeakAPIMetricReader : IMetricReader
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<WeakAPIMetricReader> logger;
    private JsonSerializerOptions _jsonOptions;

    public WeakAPIMetricReader(
        IHttpClientFactory httpClientFactory,
        ILogger<WeakAPIMetricReader> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new LocationJsonConverter());
    }

    public async Task<List<MetricReadingBase>> ReadMetricsAsync(CancellationToken ct = default)
    {
        var httpclient = httpClientFactory.CreateClient("WeakAPIClient");

        logger.LogInformation("Metrics fetch started");
        var readings = await httpclient.GetFromJsonAsync<List<MetricReadingBase>>("meters", _jsonOptions, ct);
        logger.LogInformation("Metrics fetch finished");

        return readings ?? [];
    }
}