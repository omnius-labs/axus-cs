using System;
using System.Linq;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal record MerkleTreeSectionEntity
    {
        public int Depth { get; set; }

        public uint BlockLength { get; set; }

        public ulong Length { get; set; }

        public OmniHashEntity[]? Hashes { get; set; }

        public static MerkleTreeSectionEntity Import(MerkleTreeSection value)
        {
            return new MerkleTreeSectionEntity() { Depth = value.Depth, Length = value.Length, Hashes = value.Hashes.Select(n => OmniHashEntity.Import(n)).ToArray() };
        }

        public MerkleTreeSection Export()
        {
            return new MerkleTreeSection(this.Depth, this.BlockLength, this.Length, this.Hashes?.Select(n => n.Export())?.ToArray() ?? Array.Empty<OmniHash>());
        }
    }
}
