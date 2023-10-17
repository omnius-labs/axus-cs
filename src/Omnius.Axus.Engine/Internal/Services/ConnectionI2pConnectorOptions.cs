using Omnius.Core.Net;

namespace Omnius.Axus.Core.Engine.Services;

public record ConnectionI2pConnectorOptions
{
    public required OmniAddress SamBridgeAddress { get; init; }
}
