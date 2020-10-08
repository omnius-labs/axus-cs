using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IWantContentStorageFactory
    {
        ValueTask<IWantContentStorage> CreateAsync(WantContentStorageOptions options, IBytesPool bytesPool);
    }

    public interface IWantContentStorage : IWritableContentStorage, IAsyncDisposable
    {
        ValueTask<WantContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask RegisterWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask UnregisterWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask ExportContentAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
        ValueTask ExportContentAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
    }
}
