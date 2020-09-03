using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Components.Models;

namespace Omnius.Xeus.Components.Storages
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
