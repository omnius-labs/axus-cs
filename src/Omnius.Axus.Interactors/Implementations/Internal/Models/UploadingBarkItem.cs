using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record UploadingBarkItem
{
    public OmniSignature Signature { get; init; } = OmniSignature.Empty;
    public OmniHash RootHash { get; init; }
}
