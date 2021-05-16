using System;
using System.Linq;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal record DecodedContentItemEntity
    {
        public int Id { get; set; }

        public OmniHashEntity? RootHash { get; set; }

        public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

        public static DecodedContentItemEntity Import(DecodedContentItem value)
        {
            return new DecodedContentItemEntity()
            {
                RootHash = OmniHashEntity.Import(value.RootHash),
                MerkleTreeSections = value.MerkleTreeSections?.Select(n => MerkleTreeSectionEntity.Import(n))?.ToArray() ?? Array.Empty<MerkleTreeSectionEntity>(),
            };
        }

        public DecodedContentItem Export()
        {
            return new DecodedContentItem(this.RootHash?.Export() ?? OmniHash.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>());
        }
    }
}
