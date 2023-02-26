using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public record ConnectionTcpAccepterOptions
{
    public required bool UseUpnp { get; init; }
    public required OmniAddress ListenAddress { get; init; }
}
