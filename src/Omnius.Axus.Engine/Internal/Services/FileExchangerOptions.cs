namespace Omnius.Axus.Engine.Internal.Services;

public record FileExchangerOptions
{
    public required uint MaxSessionCount { get; init; }
}
