using Omnius.Axis.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record FileSeedEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedTime { get; set; }
    public ulong Size { get; set; }

    public static FileSeedEntity Import(FileSeed value)
    {
        return new FileSeedEntity()
        {
            RootHash = OmniHashEntity.Import(value.RootHash),
            Name = value.Name,
            Size = value.Size,
            CreatedTime = value.CreatedTime.ToDateTime(),
        };
    }

    public FileSeed Export()
    {
        return new FileSeed(this.RootHash?.Export() ?? OmniHash.Empty, this.Name ?? string.Empty, this.Size, Timestamp.FromDateTime(this.CreatedTime));
    }
}
