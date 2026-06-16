using DataIngestor.Application.Interfaces;
using DataIngestor.Application.Services;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Entities.Payload;
using DataIngestor.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DataIngestor.Application.Tests;

public class MetricsIngestionServiceTests
{
    private readonly IMetricReader metricReader = Substitute.For<IMetricReader>();
    private readonly IMetricPublisher metricPublisher = Substitute.For<IMetricPublisher>();
    private readonly MetricsIngestionService sut;

    public MetricsIngestionServiceTests()
    {
        sut = new MetricsIngestionService(
            metricReader,
            metricPublisher,
            Substitute.For<ILogger<MetricsIngestionService>>());
    }

    [Fact]
    public async Task IngestAsyncWhenReaderReturnsReadingsPublishesThemAll()
    {
        // Arrange
        List<MetricReadingBase> readings =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(42.5),
            },
            new MetricReading<MotionReadingPayload>
            {
                Location = new Location("Hallway"),
                Payload = new MotionReadingPayload(true),
            },
        ];

        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .Returns(readings);

        // Act
        await sut.IngestAsync();

        // Assert
        await metricPublisher.Received(1)
            .PublishAsync(
                Arg.Is<IReadOnlyList<MetricReadingBase>>(l => l.Count == 2),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAsyncWhenReaderReturnsEmptyListPublishesEmptyBatch()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await sut.IngestAsync();

        // Assert
        await metricPublisher.Received(1)
            .PublishAsync(
                Arg.Is<IReadOnlyList<MetricReadingBase>>(l => l.Count == 0),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAsyncCallsReaderExactlyOnce()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await sut.IngestAsync();

        // Assert
        await metricReader.Received(1).ReadMetricsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAsyncCallsPublisherExactlyOnce()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await sut.IngestAsync();

        // Assert
        await metricPublisher.Received(1)
            .PublishAsync(Arg.Any<IReadOnlyList<MetricReadingBase>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAsyncPassesCancellationTokenToReaderAndPublisher()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        metricReader.ReadMetricsAsync(cts.Token).Returns([]);

        // Act
        await sut.IngestAsync(cts.Token);

        // Assert
        await metricReader.Received(1).ReadMetricsAsync(cts.Token);
        await metricPublisher.Received(1)
            .PublishAsync(Arg.Any<IReadOnlyList<MetricReadingBase>>(), cts.Token);
    }

    [Fact]
    public async Task IngestAsyncWhenReaderThrowsPropagatesException()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("upstream down"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => sut.IngestAsync());
    }

    [Fact]
    public async Task IngestAsyncWhenReaderThrowsPublisherIsNotCalled()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("upstream down"));

        // Act
        await Assert.ThrowsAsync<HttpRequestException>(() => sut.IngestAsync());

        // Assert
        await metricPublisher.DidNotReceiveWithAnyArgs()
            .PublishAsync(default!, default);
    }

    [Fact]
    public async Task IngestAsyncWhenPublisherThrowsPropagatesException()
    {
        // Arrange
        metricReader.ReadMetricsAsync(Arg.Any<CancellationToken>())
            .Returns([]);
        metricPublisher.PublishAsync(Arg.Any<IReadOnlyList<MetricReadingBase>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("broker unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.IngestAsync());
    }
}
