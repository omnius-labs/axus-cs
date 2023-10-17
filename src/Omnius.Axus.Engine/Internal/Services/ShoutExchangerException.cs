namespace Omnius.Axus.Core.Engine.Services;

public sealed class ShoutExchangerException : Exception
{
    public ShoutExchangerException()
        : base()
    {
    }

    public ShoutExchangerException(string? message)
        : base(message)
    {
    }

    public ShoutExchangerException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
