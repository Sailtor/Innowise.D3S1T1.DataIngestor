namespace DataIngestor.Domain.ValueObjects;

public record Location
{
    public Location(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Location name cannot be empty.");
        }

        Name = name;
    }

    public string Name { get; }

    public override string ToString() => Name;
}
