namespace Omnius.Xeus.Service.Engines;

public record FileExchangerOptions
{
    public FileExchangerOptions(uint maxSessionCount)
    {
        this.MaxSessionCount = maxSessionCount;
    }

    public uint MaxSessionCount { get; }
}
