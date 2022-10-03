namespace Omnius.Axus.Engines;

public record NodeLocationsFetcherOptions
{
    public NodeLocationsFetcherOptions(NodeLocationsFetcherOperationType operationType, string? uri = null)
    {
        this.OperationType = operationType;
        this.Uri = uri;
    }

    public string? Uri { get; }
    public NodeLocationsFetcherOperationType OperationType { get; }
}

public enum NodeLocationsFetcherOperationType : byte
{
    None = 0,
    HttpGet = 1,
}
