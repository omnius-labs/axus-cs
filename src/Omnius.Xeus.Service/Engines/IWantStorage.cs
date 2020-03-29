using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Primitives;

namespace Omnius.Xeus.Service.Engines
{

    public interface IWantStorageFactory
    {
        ValueTask<IWantStorage> CreateAsync(string configPath, WantStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IWantStorage : IWritableStorage
    {
        IAsyncEnumerable<WantReport> GetReportsAsync(CancellationToken cancellationToken = default);

        ValueTask WantAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

        ValueTask UnwantAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

        ValueTask ExportAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
        ValueTask ExportAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
    }
}
