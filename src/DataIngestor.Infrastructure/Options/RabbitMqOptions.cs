namespace DataIngestor.Infrastructure.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public required string Host { get; init; }

    public string VirtualHost { get; init; } = "/";

    public required string Username { get; init; }

    public required string Password { get; init; }
}
