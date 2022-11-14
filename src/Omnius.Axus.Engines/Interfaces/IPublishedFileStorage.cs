using System.Buffers;
using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface IPublishedFileStorage : IReadOnlyFileStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<OmniHash>> GetPushRootHashesAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<OmniHash>> GetPushBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    ValueTask<bool> ContainsPushContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    ValueTask<bool> ContainsPushBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);
    ValueTask<OmniHash> PublishFileAsync(string filePath, int maxBlockSize, string author, CancellationToken cancellationToken = default);
    ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, int maxBlockSize, string author, CancellationToken cancellationToken = default);
    ValueTask UnpublishFileAsync(string filePath, string author, CancellationToken cancellationToken = default);
    ValueTask UnpublishFileAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default);
}
