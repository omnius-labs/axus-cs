using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public record SessionAccepterOptions
{
    public SessionAccepterOptions(OmniDigitalSignature digitalSignature)
    {
        this.DigitalSignature = digitalSignature;
    }

    public OmniDigitalSignature DigitalSignature { get; }
}
