using Omnius.Core.Net;

namespace Omnius.Axis.Engines;

public record I2pConnectionConnectorOptions
{
    public I2pConnectionConnectorOptions(OmniAddress samBridgeAddress)
    {
        this.SamBridgeAddress = samBridgeAddress;
    }

    public OmniAddress SamBridgeAddress { get; }
}
