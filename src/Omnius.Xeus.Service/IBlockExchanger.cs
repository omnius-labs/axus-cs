using Omnius.Core;

namespace Omnius.Xeus.Service
{
    public interface IBlockExchanger
    {
        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
