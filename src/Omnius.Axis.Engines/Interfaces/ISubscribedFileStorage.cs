using System.Buffers;
using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines;

public interface ISubscribedFileStorage : IWritableFileStorage, IAsyncDisposable
{
    ValueTask<SubscribedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<OmniHash>> GetWantContentHashesAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<OmniHash>> GetWantBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> ContainsWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> ContainsWantBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);

    ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
}
