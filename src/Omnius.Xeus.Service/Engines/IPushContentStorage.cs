using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public interface IPushContentStorageFactory
    {
        ValueTask<IPushContentStorage> CreateAsync(PushContentStorageOptions options, IBytesPool bytesPool);
    }

    public interface IPushContentStorage : IReadOnlyContentStorage
    {
        ValueTask<PushContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask<OmniHash> RegisterPushContentAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask UnregisterPushContentAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
