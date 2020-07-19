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
    public interface IPublishDeclaredMessageStorageFactory
    {
        ValueTask<IPublishDeclaredMessageStorage> CreateAsync(PublishDeclaredMessageStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IPublishDeclaredMessageStorage : IPublishStorage, IReadOnlyDeclaredMessageStorage
    {
        ValueTask<PublishDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask PublishAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
        ValueTask UnpublishAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
