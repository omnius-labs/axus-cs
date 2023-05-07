using System.Buffers;
using Omnius.Axus.Messages;
using Omnius.Axus.Remoting;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IAxusServiceMediator
{
    ValueTask<ServiceConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(ServiceConfig config, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default);
    ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);
    ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, int maxBlockSize, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default);
    ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, int maxBlockSize, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default);
    ValueTask PublishShoutAsync(Shout message, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default);
    ValueTask SubscribeFileAsync(OmniHash rootHash, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default);
    ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default);
    ValueTask<IMemoryOwner<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
    ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, string channel, DateTime updatedTime, CancellationToken cancellationToken = default);
    ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string zone, CancellationToken cancellationToken = default);
    ValueTask UnpublishFileFromStorageAsync(string filePath, string zone, CancellationToken cancellationToken = default);
    ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeFileAsync(OmniHash rootHash, string zone, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default);
}
