using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engine.Internal.Services;

public record SessionAccepterOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
