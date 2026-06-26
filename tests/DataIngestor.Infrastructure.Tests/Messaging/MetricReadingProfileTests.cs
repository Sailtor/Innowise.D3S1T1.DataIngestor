using AutoMapper;
using DataIngestor.Domain.Entities;
using DataIngestor.Domain.Entities.Payload;
using DataIngestor.Domain.ValueObjects;
using DataIngestor.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataIngestor.Infrastructure.Tests.Messaging;

public class MetricReadingProfileTests
{
    private readonly IMapper mapper = CreateMapper();

    private static MapperConfiguration BuildConfiguration()
    {
        MapperConfigurationExpression expression = new();
        expression.AddProfile<MetricReadingProfile>();
        return new MapperConfiguration(expression, NullLoggerFactory.Instance);
    }

    private static IMapper CreateMapper() => BuildConfiguration().CreateMapper();

    [Fact]
    public void MapperConfigurationIsValid()
    {
        // Act & Assert
        BuildConfiguration().AssertConfigurationIsValid();
    }

    [Fact]
    public void MapsEnergyReadingPayloadToEnergyContractPayload()
    {
        // Arrange
        MetricReadingBase reading = new MetricReading<EnergyReadingPayload>
        {
            Location = new Location("Room-A"),
            Payload = new EnergyReadingPayload(10.0),
        };

        // Act
        Contracts.MetricReadingMessage message = mapper.Map<Contracts.MetricReadingMessage>(reading);

        // Assert
        Assert.IsType<Contracts.EnergyReadingPayload>(message.Payload);
    }

    [Fact]
    public void MapsMotionReadingPayloadToMotionContractPayload()
    {
        // Arrange
        MetricReadingBase reading = new MetricReading<MotionReadingPayload>
        {
            Location = new Location("Hallway"),
            Payload = new MotionReadingPayload(true),
        };

        // Act
        Contracts.MetricReadingMessage message = mapper.Map<Contracts.MetricReadingMessage>(reading);

        // Assert
        Assert.IsType<Contracts.MotionReadingPayload>(message.Payload);
    }

    [Fact]
    public void MapsLocationNameToRoom()
    {
        // Arrange
        MetricReadingBase reading = new MetricReading<EnergyReadingPayload>
        {
            Location = new Location("Office"),
            Payload = new EnergyReadingPayload(5.0),
        };

        // Act
        Contracts.MetricReadingMessage message = mapper.Map<Contracts.MetricReadingMessage>(reading);

        // Assert
        Assert.Equal("Office", message.Room);
    }

    [Fact]
    public void MapsEnergyPayloadValuesCorrectly()
    {
        // Arrange
        MetricReadingBase reading = new MetricReading<EnergyReadingPayload>
        {
            Location = new Location("Room-B"),
            Payload = new EnergyReadingPayload(99.9),
        };

        // Act
        Contracts.MetricReadingMessage message = mapper.Map<Contracts.MetricReadingMessage>(reading);

        // Assert
        Contracts.EnergyReadingPayload payload = Assert.IsType<Contracts.EnergyReadingPayload>(message.Payload);
        Assert.Equal(99.9, payload.Amount);
    }

    [Fact]
    public void MapsListOfReadingsToListOfMessages()
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
                Payload = new MotionReadingPayload(false),
            },
        ];

        // Act
        List<Contracts.MetricReadingMessage> messages = mapper.Map<List<Contracts.MetricReadingMessage>>(readings);

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.IsType<Contracts.EnergyReadingPayload>(messages[0].Payload);
        Assert.IsType<Contracts.MotionReadingPayload>(messages[1].Payload);
    }
}
