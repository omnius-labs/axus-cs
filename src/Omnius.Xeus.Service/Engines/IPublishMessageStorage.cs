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
    public interface IPublishMessageStorageFactory
    {
        ValueTask<IPublishMessageStorage> CreateAsync(PublishMessageStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IPublishMessageStorage : IPublishStorage
    {
        ValueTask<PublishMessageStorageReport> GetMessageReportAsync(CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
        ValueTask<OmniHash> PublishMessageAsync(OrientedMessage message, CancellationToken cancellationToken = default);
        ValueTask UnpublishMessageAsync(OmniHash hash, CancellationToken cancellationToken = default);
    }
}
