using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public record I2pConnectionAccepterOptions
{
    public required OmniAddress SamBridgeAddress { get; init; }
}
