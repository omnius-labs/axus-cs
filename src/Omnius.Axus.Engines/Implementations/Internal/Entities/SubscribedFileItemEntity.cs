using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record SubscribedFileItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }
    public SubscribedFileItemStatusEntity? Status { get; init; }

    public static SubscribedFileItemEntity Import(SubscribedFileItem item)
    {
        return new SubscribedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            Authors = item.Authors,
            Status = SubscribedFileItemStatusEntity.Import(item.Status),
        };
    }

    public SubscribedFileItem Export()
    {
        return new SubscribedFileItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            Authors = this.Authors ?? Array.Empty<string>(),
            Status = this.Status?.Export() ?? SubscribedFileItemStatus.Empty,
        };
    }
}

internal record SubscribedFileItemStatusEntity
{
    public int CurrentDepth { get; init; }
    public int TotalBlockCount { get; init; }
    public int DownloadedBlockCount { get; init; }
    public int State { get; init; }

    public static SubscribedFileItemStatusEntity Import(SubscribedFileItemStatus item)
    {
        return new SubscribedFileItemStatusEntity()
        {
            CurrentDepth = item.CurrentDepth,
            TotalBlockCount = item.TotalBlockCount,
            DownloadedBlockCount = item.DownloadedBlockCount,
            State = (int)item.State,
        };
    }

    public SubscribedFileItemStatus Export()
    {
        return new SubscribedFileItemStatus()
        {
            CurrentDepth = this.CurrentDepth,
            TotalBlockCount = this.TotalBlockCount,
            DownloadedBlockCount = this.DownloadedBlockCount,
            State = (SubscribedFileState)this.State
        };
    }
}
