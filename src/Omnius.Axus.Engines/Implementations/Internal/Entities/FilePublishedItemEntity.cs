using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record FilePublishedItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? FilePath { get; set; }
    public int MaxBlockSize { get; set; }
    public IReadOnlyList<AttachedProperty>? Properties { get; init; }
    public IReadOnlyList<string>? Zones { get; set; }

    public static FilePublishedItemEntity Import(FilePublishedItem item)
    {
        return new FilePublishedItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            FilePath = item.FilePath,
            MaxBlockSize = item.MaxBlockSize,
            Properties = item.Properties,
            Zones = item.Zones,
        };
    }

    public FilePublishedItem Export()
    {
        return new FilePublishedItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            FilePath = this.FilePath,
            MaxBlockSize = this.MaxBlockSize,
            Properties = this.Properties ?? Array.Empty<AttachedProperty>(),
            Zones = this.Zones ?? Array.Empty<string>(),
        };
    }
}
