using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record FileSubscribedItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }
    public SubscribedFileItemStatusEntity? Status { get; init; }

    public static FileSubscribedItemEntity Import(FileSubscribedItem item)
    {
        return new FileSubscribedItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            Authors = item.Authors,
            Status = SubscribedFileItemStatusEntity.Import(item.Status),
        };
    }

    public FileSubscribedItem Export()
    {
        return new FileSubscribedItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            Authors = this.Authors ?? Array.Empty<string>(),
            Status = this.Status?.Export() ?? new FileSubscribedItemStatus
            {
                CurrentDepth = 0,
                TotalBlockCount = 0,
                DownloadedBlockCount = 0,
                State = SubscribedFileState.Unknown
            },
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
