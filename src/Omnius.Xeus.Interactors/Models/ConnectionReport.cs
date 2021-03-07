using Omnius.Core.Network;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors.Models
{
    public record ConnectionReport
    {
        public ConnectionReport(string engineName, ConnectionHandshakeType handshakeType, OmniAddress address)
        {
            this.EngineName = engineName;
            this.HandshakeType = handshakeType;
            this.Address = address;
        }

        public string EngineName { get; }

        public ConnectionHandshakeType HandshakeType { get; }

        public OmniAddress Address { get; }
    }
}
