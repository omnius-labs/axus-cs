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
    public interface IContentPublisherFactory
    {
        ValueTask<IContentPublisher> CreateAsync(ContentPublisherOptions options, IBytesPool bytesPool);
    }

    public interface IContentPublisher : IReadOnlyContents, IAsyncDisposable
    {
        ValueTask<ContentPublisherReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishContentAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishContentAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishContentAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default);
    }
}
