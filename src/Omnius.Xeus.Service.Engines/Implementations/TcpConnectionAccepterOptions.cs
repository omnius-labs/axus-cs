using System.Collections.Generic;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Service.Engines;

public record TcpConnectionAccepterOptions
{
    public TcpConnectionAccepterOptions(bool useUpnp, OmniAddress listenAddress)
    {
        this.UseUpnp = useUpnp;
        this.ListenAddress = listenAddress;
    }

    public bool UseUpnp { get; }

    public OmniAddress ListenAddress { get; }
}