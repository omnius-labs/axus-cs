using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines;

public record SessionAccepterOptions
{
    public SessionAccepterOptions(OmniDigitalSignature digitalSignature)
    {
        this.DigitalSignature = digitalSignature;
    }

    public OmniDigitalSignature DigitalSignature { get; }
}
