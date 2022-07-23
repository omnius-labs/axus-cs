using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record DecodedFileItemEntity
{
    public OmniHashEntity? RootHash { get; set; }

    public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

    public int State { get; set; }

    public static DecodedFileItemEntity Import(DecodedFileItem item)
    {
        return new DecodedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            MerkleTreeSections = item.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            State = (int)item.State,
        };
    }

    public DecodedFileItem Export()
    {
        return new DecodedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>(), (SubscribedFileState)this.State);
    }
}
