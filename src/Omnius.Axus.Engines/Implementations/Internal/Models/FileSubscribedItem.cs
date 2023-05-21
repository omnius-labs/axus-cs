using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record FileSubscribedItem
{
    public required OmniHash RootHash { get; init; }
    public required FileSubscribedItemStatus Status { get; init; }
    public required IReadOnlyList<AttachedProperty> Properties { get; init; }
    public required IReadOnlyList<string> Zones { get; init; }
}

internal record FileSubscribedItemStatus
{
    public required int CurrentDepth { get; init; }
    public required int TotalBlockCount { get; init; }
    public required int DownloadedBlockCount { get; init; }
    public required SubscribedFileState State { get; init; }
}
