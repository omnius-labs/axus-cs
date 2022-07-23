using Omnius.Core.Collections;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record PublishedFileItem
{
    public PublishedFileItem(OmniHash rootHash, string? filePath, string registrant, MerkleTreeSection[] merkleTreeSections, int maxBlockLength)
    {
        this.RootHash = rootHash;
        this.FilePath = filePath;
        this.Registrant = registrant;
        this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
        this.MaxBlockLength = maxBlockLength;
    }

    public OmniHash RootHash { get; }

    public string? FilePath { get; }

    public string Registrant { get; }

    public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }

    public int MaxBlockLength { get; }
}
