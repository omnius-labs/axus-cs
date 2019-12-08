using Omnius.Core;

namespace Omnius.Xeus.Engine
{
    public interface IBlocksExchanger
    {
        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
