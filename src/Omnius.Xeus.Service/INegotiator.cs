using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;
using System.Collections.Generic;

namespace Omnius.Xeus.Service
{
    public interface INegotiatorFactory
    {
        ValueTask<INegotiator> CreateAsync(string configPath, NegotiatorOptions negotiatorOptions, IEnumerable<IConnector> connectors, 
            IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBytesPool bytesPool);
    }

    public interface INegotiator
    {
        public static INegotiatorFactory Factory { get; }

        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
