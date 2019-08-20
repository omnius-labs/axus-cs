using Omnix.Network;

namespace Xeus.Core.Internal.Connection
{
    public readonly struct ConnectionCreatorAcceptResult
    {
        public ConnectionCreatorAcceptResult(Cap cap, OmniAddress address)
        {
            this.Cap = cap;
            this.Address = address;
        }

        public Cap Cap { get; }
        public OmniAddress Address { get; }
    }
}

