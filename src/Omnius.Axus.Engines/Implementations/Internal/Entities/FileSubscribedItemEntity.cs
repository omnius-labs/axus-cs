using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record FileSubscribedItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public SubscribedFileItemStatusEntity? Status { get; init; }
    public IReadOnlyList<AttachedProperty>? Properties { get; init; }
    public IReadOnlyList<string>? Zones { get; set; }

    public static FileSubscribedItemEntity Import(FileSubscribedItem item)
    {
        return new FileSubscribedItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            Status = SubscribedFileItemStatusEntity.Import(item.Status),
            Properties = item.Properties,
            Zones = item.Zones,
        };
    }

    public FileSubscribedItem Export()
    {
        return new FileSubscribedItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            Status = this.Status?.Export() ?? new FileSubscribedItemStatus
            {
                CurrentDepth = 0,
                TotalBlockCount = 0,
                DownloadedBlockCount = 0,
                State = SubscribedFileState.Unknown
            },
            Properties = this.Properties ?? Array.Empty<AttachedProperty>(),
            Zones = this.Zones ?? Array.Empty<string>(),
        };
    }
}

internal record SubscribedFileItemStatusEntity
{
    public int CurrentDepth { get; init; }
    public int TotalBlockCount { get; init; }
    public int DownloadedBlockCount { get; init; }
    public int State { get; init; }

    public static SubscribedFileItemStatusEntity Import(FileSubscribedItemStatus item)
    {
        return new SubscribedFileItemStatusEntity()
        {
            CurrentDepth = item.CurrentDepth,
            TotalBlockCount = item.TotalBlockCount,
            DownloadedBlockCount = item.DownloadedBlockCount,
            State = (int)item.State,
        };
    }

    public FileSubscribedItemStatus Export()
    {
        return new FileSubscribedItemStatus()
        {
            CurrentDepth = this.CurrentDepth,
            TotalBlockCount = this.TotalBlockCount,
            DownloadedBlockCount = this.DownloadedBlockCount,
            State = (SubscribedFileState)this.State
        };
    }
}
