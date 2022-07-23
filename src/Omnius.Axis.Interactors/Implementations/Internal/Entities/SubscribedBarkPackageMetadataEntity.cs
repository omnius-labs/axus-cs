using LiteDB;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record SubscribedBarkPackageMetadataEntity
{
    [BsonId]
    public OmniSignatureEntity? Signature { get; set; }

    public DateTime UpdatedTime { get; set; }
}
