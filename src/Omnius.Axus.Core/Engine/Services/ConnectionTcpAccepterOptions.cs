using Omnius.Core.Net;

namespace Omnius.Axus.Core.Engine.Services;

public record ConnectionTcpAccepterOptions
{
    public required bool UseUpnp { get; init; }
    public required OmniAddress ListenAddress { get; init; }
}
