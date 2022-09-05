using System.Buffers;
using Omnius.Axus.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IServiceMediator
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

    ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, int maxBlockSize, string author, CancellationToken cancellationToken = default);

    ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, int maxBlockSize, string author, CancellationToken cancellationToken = default);

    ValueTask PublishShoutAsync(Shout message, string author, CancellationToken cancellationToken = default);

    ValueTask SubscribeFileAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default);

    ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);

    ValueTask<IMemoryOwner<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

    ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);

    ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default);

    ValueTask UnpublishFileFromStorageAsync(string filePath, string author, CancellationToken cancellationToken = default);

    ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeFileAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);
}
