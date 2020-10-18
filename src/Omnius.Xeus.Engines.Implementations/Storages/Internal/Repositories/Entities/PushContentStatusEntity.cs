using System;
using System.Buffers;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class PushContentStatusEntity
    {
        public string? FilePath { get; set; }
        public OmniHashEntity? Hash { get; set; }
        public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

        public static PushContentStatusEntity Import(PushContentStatus value)
        {
            return new PushContentStatusEntity()
            {
                Hash = OmniHashEntity.Import(value.Hash),
                FilePath = value.FilePath,
                MerkleTreeSections = value.MerkleTreeSections.Select(n => MerkleTreeSectionEntity.Import(n)).ToArray()
            };
        }

        public PushContentStatus Export()
        {
            return new PushContentStatus(this.Hash?.Export() ?? OmniHash.Empty, this.FilePath ?? string.Empty, this.MerkleTreeSections?.Select(n => n.Export())?.ToArray() ?? Array.Empty<MerkleTreeSection>());
        }
    }
}
