using System.Text.Json;
using DataIngestor.Domain.ValueObjects;
using DataIngestor.Infrastructure.Converters;

namespace DataIngestor.Infrastructure.Tests.Converters;

public class LocationJsonConverterTests
{
    private readonly JsonSerializerOptions options = new()
    {
        Converters = { new LocationJsonConverter() },
    };

    [Fact]
    public void ReadDeserializesStringToLocation()
    {
        // Arrange
        const string json = "\"Office\"";

        // Act
        Location? result = JsonSerializer.Deserialize<Location>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Office", result.Name);
    }

    [Fact]
    public void ReadWhenNullJsonReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act
        Location? result = JsonSerializer.Deserialize<Location?>(json, options);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WriteSerializesLocationToString()
    {
        // Arrange
        Location location = new("Hallway");

        // Act
        string json = JsonSerializer.Serialize(location, options);

        // Assert
        Assert.Equal("\"Hallway\"", json);
    }

    [Fact]
    public void RoundTripPreservesLocationName()
    {
        // Arrange
        Location original = new("Server-Room");

        // Act
        string json = JsonSerializer.Serialize(original, options);
        Location? restored = JsonSerializer.Deserialize<Location>(json, options);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.Name, restored.Name);
    }
}
