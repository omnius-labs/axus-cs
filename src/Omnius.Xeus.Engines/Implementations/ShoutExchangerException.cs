using System;

namespace Omnius.Xeus.Engines
{
    public sealed class ShoutExchangerException : Exception
    {
        public ShoutExchangerException()
            : base()
        {
        }

        public ShoutExchangerException(string message)
            : base(message)
        {
        }

        public ShoutExchangerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
