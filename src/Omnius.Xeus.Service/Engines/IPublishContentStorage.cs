using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines
{
    public interface IPublishContentStorageFactory
    {
        ValueTask<IPublishContentStorage> CreateAsync(PublishContentStorageOptions options, IBytesPool bytesPool);
    }

    public interface IPublishContentStorage : IPublishStorage, IReadOnlyContentStorage
    {
        ValueTask<PublishContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        ValueTask UnpublishAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask UnpublishAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    }
}
