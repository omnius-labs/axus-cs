namespace Omnius.Axus.Core.Engine.Services;

public record FileExchangerOptions
{
    public required uint MaxSessionCount { get; init; }
}
