using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public interface IPublishedFileStorage : IReadOnlyFileStorage, IAsyncDisposable
    {
        ValueTask<PublishedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);
    }
}
