using System;
using System.Buffers;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines.Internal.Repositories.Entities
{
    internal record PublishedFileItemEntity
    {
        public OmniHashEntity? RootHash { get; set; }

        public string? FilePath { get; set; }

        public string? Registrant { get; set; }

        public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

        public static PublishedFileItemEntity Import(PublishedFileItem value)
        {
            return new PublishedFileItemEntity()
            {
                RootHash = OmniHashEntity.Import(value.RootHash),
                FilePath = value.FilePath,
                Registrant = value.Registrant,
                MerkleTreeSections = value.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            };
        }

        public PublishedFileItem Export()
        {
            return new PublishedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty, this.FilePath ?? string.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>());
        }
    }
}
