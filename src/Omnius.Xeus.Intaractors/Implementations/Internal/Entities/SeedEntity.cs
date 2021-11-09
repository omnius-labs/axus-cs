using System;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors.Internal.Entities;

internal record SeedEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? Name { get; set; }
    public DateTime CreationTime { get; set; }
    public ulong Size { get; set; }

    public static SeedEntity Import(Seed value)
    {
        return new SeedEntity()
        {
            RootHash = OmniHashEntity.Import(value.RootHash),
            Name = value.Name,
            Size = value.Size,
            CreationTime = value.CreationTime.ToDateTime(),
        };
    }

    public Seed Export()
    {
        return new Seed(this.RootHash?.Export() ?? OmniHash.Empty, this.Name ?? string.Empty, this.Size, Timestamp.FromDateTime(this.CreationTime));
    }
}