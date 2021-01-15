using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal sealed class DecodedContentItem
    {
        public DecodedContentItem(OmniHash contentHash, MerkleTreeSection[] merkleTreeSections)
        {
            this.ContentHash = contentHash;
            this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
        }

        public OmniHash ContentHash { get; }

        public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
    }
}
