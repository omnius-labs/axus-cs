using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal record DecodedContentItem
    {
        public DecodedContentItem(OmniHash rootHash, MerkleTreeSection[] merkleTreeSections)
        {
            this.RootHash = rootHash;
            this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
        }

        public OmniHash RootHash { get; }

        public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
    }
}
