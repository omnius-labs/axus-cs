using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IContentSubscriberFactory
    {
        ValueTask<IContentSubscriber> CreateAsync(ContentSubscriberOptions options, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IContentSubscriber : IWritableContents, IAsyncDisposable
    {
        ValueTask<ContentSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeContentAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeContentAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask<bool> ExportContentAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

        ValueTask<bool> ExportContentAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
    }
}
