using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedFileItem
{
    public OmniHash RootHash { get; init; }
    public IReadOnlyList<string> Authors { get; init; } = Array.Empty<string>();
    public SubscribedFileItemStatus Status { get; init; } = SubscribedFileItemStatus.Empty;
}

internal record SubscribedFileItemStatus
{
    public static SubscribedFileItemStatus Empty { get; } = new SubscribedFileItemStatus();
    public int CurrentDepth { get; init; }
    public int TotalBlockCount { get; init; }
    public int DownloadedBlockCount { get; init; }
    public SubscribedFileState State { get; init; }
}
