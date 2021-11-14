using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines.Primitives;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines;

public interface ISubscribedShoutStorage : IWritableShoutStorage, IAsyncDisposable
{
    ValueTask<SubscribedShoutStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
}
