using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IWantContentStorageFactory
    {
        ValueTask<IWantContentStorage> CreateAsync(WantContentStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IWantContentStorage : IWantStorage, IWritableContentStorage
    {
        ValueTask<WantContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask WantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask UnwantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask ExportContentAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
        ValueTask ExportContentAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
    }
}
