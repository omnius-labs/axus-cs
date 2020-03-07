using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IWantFileStorageFactory
    {
        ValueTask<IWantFileStorage> CreateAsync(string configPath, IBytesPool bytesPool);
    }

    public interface IWantFileStorage : IWantStorage, IAsyncDisposable
    {
        ValueTask AddWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask RemoveWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask<bool> TryExportWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
    }
}
