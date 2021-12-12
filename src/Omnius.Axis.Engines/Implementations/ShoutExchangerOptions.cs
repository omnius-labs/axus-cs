namespace Omnius.Axis.Engines;

public record ShoutExchangerOptions
{
    public ShoutExchangerOptions(uint maxSessionCount)
    {
        this.MaxSessionCount = maxSessionCount;
    }

    public uint MaxSessionCount { get; }
}
