using System;
using System.Buffers;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class ContentPublisherItemEntity
    {
        public string? FilePath { get; set; }

        public OmniHashEntity? ContentHash { get; set; }

        public string? Registrant { get; set; }

        public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

        public static ContentPublisherItemEntity Import(ContentPublisherItem value)
        {
            return new ContentPublisherItemEntity()
            {
                ContentHash = OmniHashEntity.Import(value.ContentHash),
                FilePath = value.FilePath,
                Registrant = value.Registrant,
                MerkleTreeSections = value.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            };
        }

        public ContentPublisherItem Export()
        {
            return new ContentPublisherItem(this.ContentHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty, this.FilePath ?? string.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>());
        }
    }
}
