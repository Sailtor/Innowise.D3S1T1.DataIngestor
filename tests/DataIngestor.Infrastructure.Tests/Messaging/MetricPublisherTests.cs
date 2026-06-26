using AutoMapper;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Entities.Payload;
using DataIngestor.Domain.ValueObjects;
using DataIngestor.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DataIngestor.Infrastructure.Tests.Messaging;

public class MetricPublisherTests
{
    private readonly IPublishEndpoint publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly IMapper mapper = CreateMapper();
    private readonly MetricPublisher sut;

    public MetricPublisherTests()
    {
        sut = new MetricPublisher(publishEndpoint, mapper);
    }

    [Fact]
    public async Task PublishAsyncPublishesOneBatchMessage()
    {
        // Arrange
        List<MetricReadingBase> readings =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(42.0),
            },
        ];

        // Act
        await sut.PublishAsync(readings, TestContext.Current.CancellationToken);

        // Assert
        await publishEndpoint.Received(1)
            .Publish(Arg.Any<Contracts.MetricReadingsBatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsyncBatchContainsAllMappedMessages()
    {
        // Arrange
        List<MetricReadingBase> readings =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(1.0),
            },
            new MetricReading<MotionReadingPayload>
            {
                Location = new Location("Hallway"),
                Payload = new MotionReadingPayload(true),
            },
        ];

        // Act
        await sut.PublishAsync(readings, TestContext.Current.CancellationToken);

        // Assert
        await publishEndpoint.Received(1)
            .Publish(
                Arg.Is<Contracts.MetricReadingsBatch>(b => b.Readings.Count == 2),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsyncBatchIngestedAtIsUtc()
    {
        // Arrange
        List<MetricReadingBase> readings =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(1.0),
            },
        ];

        DateTime before = DateTime.UtcNow;

        // Act
        await sut.PublishAsync(readings, TestContext.Current.CancellationToken);

        DateTime after = DateTime.UtcNow;

        // Assert
        await publishEndpoint.Received(1)
            .Publish(
                Arg.Is<Contracts.MetricReadingsBatch>(b => b.IngestedAtUtc >= before && b.IngestedAtUtc <= after),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsyncPassesCancellationTokenToEndpoint()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        List<MetricReadingBase> readings =
        [
            new MetricReading<EnergyReadingPayload>
            {
                Location = new Location("Room-A"),
                Payload = new EnergyReadingPayload(1.0),
            },
        ];

        // Act
        await sut.PublishAsync(readings, cts.Token);

        // Assert
        await publishEndpoint.Received(1)
            .Publish(Arg.Any<Contracts.MetricReadingsBatch>(), cts.Token);
    }

    [Fact]
    public async Task PublishAsyncWhenEmptyReadingsPublishesEmptyBatch()
    {
        // Act
        await sut.PublishAsync([], TestContext.Current.CancellationToken);

        // Assert
        await publishEndpoint.Received(1)
            .Publish(
                Arg.Is<Contracts.MetricReadingsBatch>(b => b.Readings.Count == 0),
                Arg.Any<CancellationToken>());
    }

    private static IMapper CreateMapper()
    {
        MapperConfigurationExpression expression = new();
        expression.AddProfile<MetricReadingProfile>();
        return new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();
    }
}
