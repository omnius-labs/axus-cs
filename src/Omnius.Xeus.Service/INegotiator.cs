using Omnius.Core;

namespace Omnius.Xeus.Service
{
    public interface INegotiator
    {
        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
