using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Entities.Payload;
using DataIngestor.Domain.ValueObjects;
using DataIngestor.Infrastructure.APIClients;
using DataIngestor.Infrastructure.Converters;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace DataIngestor.Infrastructure.Tests.APIClients;

public class WeakAPIMetricReaderTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new LocationJsonConverter() },
    };

    private static WeakAPIMetricReader BuildSut(MockHttpMessageHandler mockHttp)
    {
        HttpClient client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("https://weak-api.test/");

        return new WeakAPIMetricReader(client, Substitute.For<ILogger<WeakAPIMetricReader>>());
    }

    [Fact]
    public async Task ReadMetricsAsyncReturnsDeserializedReadings()
    {
        // Arrange
        List<MetricReadingBase> expected =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(42.5),
            },
        ];

        string json = JsonSerializer.Serialize(expected, SerializerOptions);

        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond("application/json", json);

        // Act
        List<MetricReadingBase> result = await BuildSut(mockHttp).ReadMetricsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Room-A", result[0].Location.Name);
    }

    [Fact]
    public async Task ReadMetricsAsyncWhenResponseIsEmptyArrayReturnsEmptyList()
    {
        // Arrange
        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond("application/json", "[]");

        // Act
        List<MetricReadingBase> result = await BuildSut(mockHttp).ReadMetricsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadMetricsAsyncWhenResponseIsNullReturnsEmptyList()
    {
        // Arrange
        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond("application/json", "null");

        // Act
        List<MetricReadingBase> result = await BuildSut(mockHttp).ReadMetricsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadMetricsAsyncDeserializesMultipleReadingTypes()
    {
        // Arrange
        string json =
            """
            [
              { "type": "energy", "name": "Room-A", "payload": { "energy": 10.0 } },
              { "type": "motion", "name": "Hallway", "payload": { "motionDetected": true } }
            ]
            """;

        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond("application/json", json);

        // Act
        List<MetricReadingBase> result = await BuildSut(mockHttp).ReadMetricsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<MetricReading<EnergyReadingPayload>>(result[0]);
        Assert.IsType<MetricReading<MotionReadingPayload>>(result[1]);
    }

    [Fact]
    public async Task ReadMetricsAsyncWhenHttpFailsThrowsHttpRequestException()
    {
        // Arrange
        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => BuildSut(mockHttp).ReadMetricsAsync());
    }

    [Fact]
    public async Task ReadMetricsAsyncHonoursCancellationToken()
    {
        // Arrange
        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://weak-api.test/meters")
            .Respond("application/json", "[]");

        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => BuildSut(mockHttp).ReadMetricsAsync(cts.Token));
    }
}
