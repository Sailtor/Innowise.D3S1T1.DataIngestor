using DataIngestor.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataIngestor.Infrastructure.Converters;

public class LocationJsonConverter : JsonConverter<Location>
{
    public override Location? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        return name == null ? null : new Location(name);
    }

    public override void Write(Utf8JsonWriter writer, Location value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
