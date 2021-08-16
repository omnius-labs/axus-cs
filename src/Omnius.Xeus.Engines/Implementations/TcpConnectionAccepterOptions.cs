using System.Collections.Generic;
using Omnius.Core.Net;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Engines
{
    public record TcpConnectionAccepterOptions
    {
        public bool UseUpnp { get; init; }

        public IReadOnlyCollection<OmniAddress>? ListenAddresses { get; init; }

        public IUpnpClientFactory? UpnpClientFactory { get; init; }

        public IBatchActionDispatcher? BatchActionDispatcher { get; init; }
    }
}
