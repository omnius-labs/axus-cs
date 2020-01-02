using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;

namespace Omnius.Xeus.Service.Primitives
{
    public enum ConnectorResultType
    {
        Succeeded,
        Failed,
    }

    public readonly struct ConnectorResult
    {
        public ConnectorResult(ConnectorResultType type, ICap? cap = null, OmniAddress? address = null)
        {
            this.Type = type;
            this.Cap = cap;
            this.Address = address;
        }

        public ConnectorResultType Type { get; }
        public ICap? Cap { get; }
        public OmniAddress? Address { get; }
    }

    public interface IPrimitiveConnector
    {
        ValueTask<ConnectorResult> AcceptAsync(CancellationToken cancellationToken = default);
        ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default);
    }
}
