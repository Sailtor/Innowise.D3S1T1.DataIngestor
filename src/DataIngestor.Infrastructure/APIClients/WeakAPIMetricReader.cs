using System.Net.Http.Json;
using System.Text.Json;
using DataIngestor.Application.Interfaces;
using DataIngestor.Domain.Entities;
using DataIngestor.Infrastructure.Converters;
using Microsoft.Extensions.Logging;

namespace DataIngestor.Infrastructure.APIClients;

public class WeakAPIMetricReader : IMetricReader
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<WeakAPIMetricReader> logger;
    private JsonSerializerOptions jsonOptions;

    public WeakAPIMetricReader(
        IHttpClientFactory httpClientFactory,
        ILogger<WeakAPIMetricReader> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        jsonOptions.Converters.Add(new LocationJsonConverter());
    }

    public async Task<List<MetricReadingBase>> ReadMetricsAsync(CancellationToken cancellationToken = default)
    {
        var httpclient = httpClientFactory.CreateClient("WeakAPIClient");

        logger.LogInformation("Metrics fetch started");
        var readings = await httpclient.GetFromJsonAsync<List<MetricReadingBase>>("meters", this.jsonOptions, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Metrics fetch finished");

        return readings ?? [];
    }
}