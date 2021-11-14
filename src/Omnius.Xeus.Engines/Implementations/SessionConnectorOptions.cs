using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines;

public record SessionConnectorOptions
{
    public SessionConnectorOptions(OmniDigitalSignature digitalSignature)
    {
        this.DigitalSignature = digitalSignature;
    }

    public OmniDigitalSignature DigitalSignature { get; }
}
