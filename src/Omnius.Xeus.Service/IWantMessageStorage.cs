using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IWantMessageStorageFactory
    {
        ValueTask<IWantMessageStorage> CreateAsync(string configPath, IBytesPool bytesPool);
    }

    public interface IWantMessageStorage : IWantStorage, IAsyncDisposable
    {
        ValueTask AddWantMessageAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask RemoveWantMessageAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask<bool> TryExportWantMessageAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
    }
}
