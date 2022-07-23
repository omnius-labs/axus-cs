namespace Omnius.Axus.Engines;

public record FileExchangerOptions
{
    public FileExchangerOptions(uint maxSessionCount)
    {
        this.MaxSessionCount = maxSessionCount;
    }

    public uint MaxSessionCount { get; }
}
