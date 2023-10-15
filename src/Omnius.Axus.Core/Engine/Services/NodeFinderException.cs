public sealed class NodeFinderException : Exception
{
    public NodeFinderException()
        : base()
    {
    }

    public NodeFinderException(string? message)
        : base(message)
    {
    }

    public NodeFinderException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
