using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public record I2pConnectionConnectorOptions
{
    public required OmniAddress SamBridgeAddress { get; init; }
}
