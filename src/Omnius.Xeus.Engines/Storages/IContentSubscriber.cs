using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IContentSubscriberFactory
    {
        ValueTask<IContentSubscriber> CreateAsync(ContentSubscriberOptions options, IBytesPool bytesPool);
    }

    public interface IContentSubscriber : IWritableContents, IAsyncDisposable
    {
        ValueTask<ContentSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask ExportContentAsync(OmniHash contentHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);

        ValueTask ExportContentAsync(OmniHash contentHash, string filePath, CancellationToken cancellationToken = default);
    }
}
