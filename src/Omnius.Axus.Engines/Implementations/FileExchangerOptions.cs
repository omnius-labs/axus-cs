namespace Omnius.Axus.Engines;

public record FileExchangerOptions
{
    public required uint MaxSessionCount { get; init; }
}
