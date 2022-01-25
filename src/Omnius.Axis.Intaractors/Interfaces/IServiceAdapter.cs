using Omnius.Axis.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors;

public interface IServiceAdapter
{
    ValueTask<ServiceConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ServiceConfig config, CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default);

    ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);

    ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, string registrant, CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default);

    ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

    ValueTask<ReadOnlyMemory<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

    ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
}
