using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal sealed class PushContentStatus
    {
        public PushContentStatus(OmniHash hash, string filePath, MerkleTreeSection[] merkleTreeSections)
        {
            this.FilePath = filePath;
            this.Hash = hash;
            this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
        }

        public string FilePath { get; }
        public OmniHash Hash { get; }
        public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
    }
}
