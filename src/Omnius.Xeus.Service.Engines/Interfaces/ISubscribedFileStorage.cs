using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines.Primitives;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public interface ISubscribedFileStorage : IWritableFileStorage, IAsyncDisposable
    {
        ValueTask<SubscribedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask<bool> ExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

        ValueTask<bool> ExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
    }
}
