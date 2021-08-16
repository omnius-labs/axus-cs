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
        ValueTask<SubscribedShoutReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
    }
}
