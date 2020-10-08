using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IPushDeclaredMessageStorageFactory
    {
        ValueTask<IPushDeclaredMessageStorage> CreateAsync(PushDeclaredMessageStorageOptions options, IBytesPool bytesPool);
    }

    public interface IPushDeclaredMessageStorage : IReadOnlyDeclaredMessageStorage, IAsyncDisposable
    {
        ValueTask<PushDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask RegisterPushMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
        ValueTask UnregisterPushMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
