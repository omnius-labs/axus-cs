using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record SeedEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedTime { get; set; }
    public ulong Size { get; set; }

    public static SeedEntity Import(Seed item)
    {
        return new SeedEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            Name = item.Name,
            Size = item.Size,
            CreatedTime = item.CreatedTime.ToDateTime(),
        };
    }

    public Seed Export()
    {
        return new Seed(this.RootHash?.Export() ?? OmniHash.Empty, this.Name ?? string.Empty, this.Size, Timestamp64.FromDateTime(this.CreatedTime));
    }
}
