using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Intaractors
{
    public interface IXeusApi
    {
        ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);

        ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFilesReportAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutsReportAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<SessionReport>> GetSessionsReportAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFilesReportAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutsReportAsync(CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, string registrant, CancellationToken cancellationToken = default);

        ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default);

        ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

        ValueTask<bool> TryExportFileToMemoryAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);

        ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

        ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
    }
}
