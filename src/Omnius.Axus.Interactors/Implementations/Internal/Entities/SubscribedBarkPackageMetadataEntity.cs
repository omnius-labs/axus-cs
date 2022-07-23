using LiteDB;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record SubscribedBarkPackageMetadataEntity
{
    [BsonId]
    public OmniSignatureEntity? Signature { get; set; }

    public DateTime UpdatedTime { get; set; }
}
