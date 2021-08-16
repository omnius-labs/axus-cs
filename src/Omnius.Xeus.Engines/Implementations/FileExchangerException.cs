using System;

namespace Omnius.Xeus.Engines.Exchangers
{
    public sealed class FileExchangerException : Exception
    {
        public FileExchangerException()
            : base()
        {
        }

        public FileExchangerException(string message)
            : base(message)
        {
        }

        public FileExchangerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
