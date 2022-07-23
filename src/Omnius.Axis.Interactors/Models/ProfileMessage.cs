using System.Collections.Immutable;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Models;

public record ProfileMessage
{
    public ProfileMessage(OmniSignature signature, DateTime createdTime, IEnumerable<OmniSignature> trustedSignatures, IEnumerable<OmniSignature> blockedSignatures)
    {
        this.Signature = signature;
        this.CreatedTime = createdTime;
        this.TrustedSignatures = trustedSignatures.ToImmutableList();
        this.BlockedSignatures = blockedSignatures.ToImmutableList();
    }

    public OmniSignature Signature { get; }
    public DateTime CreatedTime { get; }
    public IReadOnlyList<OmniSignature> TrustedSignatures { get; }
    public IReadOnlyList<OmniSignature> BlockedSignatures { get; }
}
