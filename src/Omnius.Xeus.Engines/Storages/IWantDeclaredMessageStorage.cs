using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IWantDeclaredMessageStorageFactory
    {
        ValueTask<IWantDeclaredMessageStorage> CreateAsync(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool);
    }

    public interface IWantDeclaredMessageStorage : IWritableDeclaredMessageStorage, IAsyncDisposable
    {
        ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask RegisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default);
        ValueTask UnregisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
