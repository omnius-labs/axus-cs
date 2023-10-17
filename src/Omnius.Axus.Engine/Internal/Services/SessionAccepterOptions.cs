using Omnius.Core.Cryptography;

namespace Omnius.Axus.Core.Engine.Services;

public record SessionAccepterOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
