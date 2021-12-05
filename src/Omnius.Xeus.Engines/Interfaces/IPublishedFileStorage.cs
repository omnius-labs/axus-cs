using System.Buffers;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines;

public interface IPublishedFileStorage : IReadOnlyFileStorage, IAsyncDisposable
{
    ValueTask<PublishedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<OmniHash>> GetPushContentHashesAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<OmniHash>> GetPushBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> ContainsPushContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> ContainsPushBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);
}
