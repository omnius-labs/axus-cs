using System.Buffers;
using LiteDB;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Internal.Entities;

internal record PublishedFileItemEntity
{
    public OmniHashEntity? RootHash { get; set; }

    public string? FilePath { get; set; }

    public string? Registrant { get; set; }

    public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

    public int MaxBlockLength { get; set; }

    public static PublishedFileItemEntity Import(PublishedFileItem item)
    {
        return new PublishedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            FilePath = item.FilePath,
            Registrant = item.Registrant,
            MerkleTreeSections = item.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            MaxBlockLength = item.MaxBlockLength,
        };
    }

    public PublishedFileItem Export()
    {
        return new PublishedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.FilePath ?? string.Empty, this.Registrant ?? string.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>(), this.MaxBlockLength);
    }
}
