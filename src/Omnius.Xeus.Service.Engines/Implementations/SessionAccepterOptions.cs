using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines;

public record SessionAccepterOptions
{
    public SessionAccepterOptions(OmniDigitalSignature digitalSignature)
    {
        this.DigitalSignature = digitalSignature;
    }

    public OmniDigitalSignature DigitalSignature { get; }
}
