namespace Omnius.Axus.Engine.Internal.Services;

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
