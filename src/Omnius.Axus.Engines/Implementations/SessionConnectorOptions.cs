using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public record SessionConnectorOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
