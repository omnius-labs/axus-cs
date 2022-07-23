using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record FileSeedEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedTime { get; set; }
    public ulong Size { get; set; }

    public static FileSeedEntity Import(FileSeed item)
    {
        return new FileSeedEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            Name = item.Name,
            Size = item.Size,
            CreatedTime = item.CreatedTime.ToDateTime(),
        };
    }

    public FileSeed Export()
    {
        return new FileSeed(this.RootHash?.Export() ?? OmniHash.Empty, this.Name ?? string.Empty, this.Size, Timestamp.FromDateTime(this.CreatedTime));
    }
}
