using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IPushContentStorageFactory
    {
        ValueTask<IPushContentStorage> CreateAsync(PushContentStorageOptions options, IBytesPool bytesPool);
    }

    public interface IPushContentStorage : IReadOnlyContentStorage, IAsyncDisposable
    {
        ValueTask<PushContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask<OmniHash> RegisterPushContentAsync(string filePath, CancellationToken cancellationToken = default);

        ValueTask UnregisterPushContentAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
