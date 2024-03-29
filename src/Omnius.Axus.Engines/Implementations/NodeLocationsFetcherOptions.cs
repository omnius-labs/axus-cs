namespace Omnius.Axus.Engines;

public record NodeLocationsFetcherOptions
{
    public string? Uri { get; init; }
    public required NodeLocationsFetcherOperationType OperationType { get; init; }
}

public enum NodeLocationsFetcherOperationType : byte
{
    None = 0,
    HttpGet = 1,
}
