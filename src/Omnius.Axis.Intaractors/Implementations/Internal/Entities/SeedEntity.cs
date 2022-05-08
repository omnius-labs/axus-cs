using Omnius.Axis.Intaractors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axis.Intaractors.Internal.Entities;

internal record SeedEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedTime { get; set; }
    public ulong Size { get; set; }

    public static SeedEntity Import(Seed value)
    {
        return new SeedEntity()
        {
            RootHash = OmniHashEntity.Import(value.RootHash),
            Name = value.Name,
            Size = value.Size,
            CreatedTime = value.CreatedTime.ToDateTime(),
        };
    }

    public Seed Export()
    {
        return new Seed(this.RootHash?.Export() ?? OmniHash.Empty, this.Name ?? string.Empty, this.Size, Timestamp.FromDateTime(this.CreatedTime));
    }
}
