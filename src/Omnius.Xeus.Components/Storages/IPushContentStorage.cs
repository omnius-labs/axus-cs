using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Components.Storages.Primitives;

namespace Omnius.Xeus.Components.Storages
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
