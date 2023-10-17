using Omnius.Core.Cryptography;

namespace Omnius.Axus.Core.Engine.Services;

public record SessionConnectorOptions
{
    public required OmniDigitalSignature DigitalSignature { get; init; }
}
