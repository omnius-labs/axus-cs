using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public interface ISubscribedShoutStorage : IWritableShoutStorage, IAsyncDisposable
    {
        ValueTask<SubscribedShoutStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
    }
}
