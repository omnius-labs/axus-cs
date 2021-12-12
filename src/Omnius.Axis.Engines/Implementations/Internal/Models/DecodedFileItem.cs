using Omnius.Axis.Models;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Internal.Models;

internal record DecodedFileItem
{
    public DecodedFileItem(OmniHash rootHash, MerkleTreeSection[] merkleTreeSections, SubscribedFileState state)
    {
        this.RootHash = rootHash;
        this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
        this.State = state;
    }

    public OmniHash RootHash { get; }
    public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
    public SubscribedFileState State { get; }
}
