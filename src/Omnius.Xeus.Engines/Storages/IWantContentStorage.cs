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

        ValueTask RegisterWantContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default);

        ValueTask UnregisterWantContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default);

        ValueTask ExportContentAsync(OmniHash contentHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);

        ValueTask ExportContentAsync(OmniHash contentHash, string filePath, CancellationToken cancellationToken = default);
    }
}
