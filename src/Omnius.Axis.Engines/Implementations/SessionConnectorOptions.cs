using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines;

public record SessionConnectorOptions
{
    public SessionConnectorOptions(OmniDigitalSignature digitalSignature)
    {
        this.DigitalSignature = digitalSignature;
    }

    public OmniDigitalSignature DigitalSignature { get; }
}
