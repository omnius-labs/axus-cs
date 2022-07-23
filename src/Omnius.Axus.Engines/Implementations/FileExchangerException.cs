namespace Omnius.Axus.Engines;

public sealed class FileExchangerException : Exception
{
    public FileExchangerException()
        : base()
    {
    }

    public FileExchangerException(string? message)
        : base(message)
    {
    }

    public FileExchangerException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
