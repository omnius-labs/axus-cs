using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record OmniSignatureEntity
{
    public string? Name { get; set; }

    public OmniHashEntity? Hash { get; set; }

    public static OmniSignatureEntity Import(OmniSignature item)
    {
        return new OmniSignatureEntity() { Name = item.Name, Hash = OmniHashEntity.Import(item.Hash) };
    }

    public OmniSignature Export()
    {
        return new OmniSignature(this.Name!, this.Hash!.Export());
    }
}
