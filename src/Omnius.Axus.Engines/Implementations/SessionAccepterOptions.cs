using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public record SessionAccepterOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
