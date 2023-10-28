using Omnius.Core.Net;

namespace Omnius.Axus.Engine.Internal.Services;

public record ConnectionI2pAccepterOptions
{
    public required OmniAddress SamBridgeAddress { get; init; }
}
