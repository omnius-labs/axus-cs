using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines.Internal.Models;

internal record DecodedFileItem
{
    public DecodedFileItem(OmniHash rootHash, MerkleTreeSection[] merkleTreeSections)
    {
        this.RootHash = rootHash;
        this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
    }

    public OmniHash RootHash { get; }

    public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
}
