using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public record I2pConnectionAccepterOptions
{
    public I2pConnectionAccepterOptions(OmniAddress samBridgeAddress)
    {
        this.SamBridgeAddress = samBridgeAddress;
    }

    public OmniAddress SamBridgeAddress { get; }
}
