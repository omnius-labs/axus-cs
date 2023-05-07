using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record FilePublishedItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? FilePath { get; set; }
    public int MaxBlockSize { get; set; }
    public IReadOnlyList<string>? Zones { get; set; }
    public IReadOnlyList<AttachedProperty>? Properties { get; init; }

    public static FilePublishedItemEntity Import(FilePublishedItem item)
    {
        return new FilePublishedItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            FilePath = item.FilePath,
            MaxBlockSize = item.MaxBlockSize,
            Zones = item.Zones,
            Properties = item.Properties,
        };
    }

    public FilePublishedItem Export()
    {
        return new FilePublishedItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            FilePath = this.FilePath,
            MaxBlockSize = this.MaxBlockSize,
            Zones = this.Zones ?? Array.Empty<string>(),
            Properties = this.Properties ?? Array.Empty<AttachedProperty>(),
        };
    }
}
