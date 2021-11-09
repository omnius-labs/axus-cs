using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines.Primitives;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines;

public interface IPublishedFileStorage : IReadOnlyFileStorage, IAsyncDisposable
{
    ValueTask<PublishedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);
}