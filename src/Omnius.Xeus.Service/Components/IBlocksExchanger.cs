using Omnius.Core;

namespace Omnius.Xeus.Service.Components
{
    public interface IBlocksExchanger
    {
        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
