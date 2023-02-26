using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public record ConnectionI2pConnectorOptions
{
    public required OmniAddress SamBridgeAddress { get; init; }
}
