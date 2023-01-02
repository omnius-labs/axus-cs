using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedFileItem
{
    public required OmniHash RootHash { get; init; }
    public required IReadOnlyList<string> Authors { get; init; }
    public required SubscribedFileItemStatus Status { get; init; }
}

internal record SubscribedFileItemStatus
{
    public required int CurrentDepth { get; init; }
    public required int TotalBlockCount { get; init; }
    public required int DownloadedBlockCount { get; init; }
    public required SubscribedFileState State { get; init; }
}
