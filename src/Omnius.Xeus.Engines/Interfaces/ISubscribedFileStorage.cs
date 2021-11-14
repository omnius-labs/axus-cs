using System.Buffers;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines;

public interface ISubscribedFileStorage : IWritableFileStorage, IAsyncDisposable
{
    ValueTask<SubscribedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
}
