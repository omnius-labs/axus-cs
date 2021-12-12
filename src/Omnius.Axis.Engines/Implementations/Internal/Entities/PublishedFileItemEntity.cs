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

    public static PublishedFileItemEntity Import(PublishedFileItem value)
    {
        return new PublishedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(value.RootHash),
            FilePath = value.FilePath,
            Registrant = value.Registrant,
            MerkleTreeSections = value.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            MaxBlockLength = value.MaxBlockLength,
        };
    }

    public PublishedFileItem Export()
    {
        return new PublishedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.FilePath ?? string.Empty, this.Registrant ?? string.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>(), this.MaxBlockLength);
    }
}
