using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record MerkleTreeSectionEntity
{
    public int Depth { get; set; }
    public uint BlockLength { get; set; }
    public ulong Length { get; set; }
    public OmniHashEntity[]? Hashes { get; set; }

    public static MerkleTreeSectionEntity Import(MerkleTreeSection item)
    {
        return new MerkleTreeSectionEntity()
        {
            Depth = item.Depth,
            Hashes = item.Hashes.Select(n => OmniHashEntity.Import(n)).ToArray(),
        };
    }

    public MerkleTreeSection Export()
    {
        return new MerkleTreeSection(this.Depth, this.Hashes?.Select(n => n.Export())?.ToArray() ?? Array.Empty<OmniHash>());
    }
}
