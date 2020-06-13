using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Primitives;

namespace Omnius.Xeus.Service.Engines
{
    public interface IPublishStorageFactory
    {
        ValueTask<IPublishStorage> CreateAsync(string configPath, PublishStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IPublishStorage : IReadOnlyStorage
    {
        ValueTask<PublishReport[]> GetReportsAsync(CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);

        ValueTask UnpublishAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask UnpublishAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    }
}
