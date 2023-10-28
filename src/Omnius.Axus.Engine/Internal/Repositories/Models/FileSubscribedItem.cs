using Omnius.Axus.Engine.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engine.Internal.Repositories.Models;

internal record FileSubscribedItem
{
    public required OmniHash RootHash { get; init; }
    public required FileSubscribedItemStatus Status { get; init; }
    public required AttachedProperty? Property { get; init; }
    public required DateTime CreatedTime { get; init; }
    public required DateTime UpdatedTime { get; init; }
}

internal record FileSubscribedItemStatus
{
    public required int CurrentDepth { get; init; }
    public required int TotalBlockCount { get; init; }
    public required int DownloadedBlockCount { get; init; }
    public required SubscribedFileState State { get; init; }
}
