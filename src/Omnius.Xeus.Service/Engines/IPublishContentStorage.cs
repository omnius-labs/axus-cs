using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IPublishContentStorageFactory
    {
        ValueTask<IPublishContentStorage> CreateAsync(PublishContentStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IPublishContentStorage : IPublishStorage, IReadOnlyContentStorage
    {
        ValueTask<PublishContentStorageReport[]> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishContentAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishContentAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        ValueTask UnpublishContentAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask UnpublishContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    }
}
