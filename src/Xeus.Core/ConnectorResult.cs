using Omnix.Network;

namespace Xeus.Core
{
    public enum ConnectorResultType
    {
        Succeeded,
        Failed,
    }

    public readonly struct ConnectorResult
    {
        public ConnectorResult(ConnectorResultType type, Cap? cap, OmniAddress? address)
        {
            this.Type = type;
            this.Cap = cap;
            this.Address = address;
        }

        public ConnectorResultType Type { get; }
        public Cap? Cap { get; }
        public OmniAddress? Address { get; }
    }
}
