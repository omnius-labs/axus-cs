using Omnix.Network;
using Omnix.Network.Caps;

namespace Xeus.Core
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
}
