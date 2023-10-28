using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engine.Internal.Services;

public record SessionConnectorOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
