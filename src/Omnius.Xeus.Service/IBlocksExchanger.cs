using Omnius.Core;

namespace Omnius.Xeus.Service
{
    public interface IBlocksExchanger
    {
        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
