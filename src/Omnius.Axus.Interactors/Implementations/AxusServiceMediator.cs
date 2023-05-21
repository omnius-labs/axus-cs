using System.Buffers;
using Omnius.Axus.Messages;
using Omnius.Axus.Remoting;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Interactors;

public sealed class AxusServiceMediator : IAxusServiceMediator
{
    private readonly IAxusService _axusService;

    public AxusServiceMediator(IAxusService axusService)
    {
        _axusService = axusService;
    }

    public async ValueTask<ServiceConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetConfigAsync(cancellationToken);
        return output.Config;
    }

    public async ValueTask SetConfigAsync(ServiceConfig config, CancellationToken cancellationToken = default)
    {
        var input = new SetConfigParam(config);
        await _axusService.SetConfigAsync(input);
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetSessionsReportAsync(cancellationToken);
        return output.Sessions;
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetMyNodeLocationAsync(cancellationToken);
        return output.NodeLocation;
    }

    public async ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetCloudNodeLocationsAsync(cancellationToken);
        return output.NodeLocations;
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        var input = new AddCloudNodeLocationsParam(nodeLocations.ToArray());
        await _axusService.AddCloudNodeLocationsAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        var input = new GetPublishedFilesReportParam(zone);
        var output = await _axusService.GetPublishedFilesReportAsync(input, cancellationToken);
        return output.PublishedFiles;
    }

    public async ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, int maxBlockSize, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromStorageParam(filePath, maxBlockSize, properties.ToArray(), zone);
        var output = await _axusService.PublishFileFromStorageAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, int maxBlockSize, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromMemoryParam(memory, maxBlockSize, properties.ToArray(), zone);
        var output = await _axusService.PublishFileFromMemoryAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask UnpublishFileFromStorageAsync(string filePath, string zone, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromStorageParam(filePath, zone);
        await _axusService.UnpublishFileFromStorageAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string zone, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromMemoryParam(rootHash, zone);
        await _axusService.UnpublishFileFromMemoryAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        var input = new GetSubscribedFilesReportParam(zone);
        var output = await _axusService.GetSubscribedFilesReportAsync(input, cancellationToken);
        return output.SubscribedFiles;
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeFileParam(rootHash, properties.ToArray(), zone);
        await _axusService.SubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string zone, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeFileParam(rootHash, zone);
        await _axusService.UnsubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToStorageParam(rootHash, filePath);
        var output = await _axusService.TryExportFileToStorageAsync(input, cancellationToken);
        return output.Success;
    }

    public async ValueTask<IMemoryOwner<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToMemoryParam(rootHash);
        var output = await _axusService.TryExportFileToMemoryAsync(input, cancellationToken);
        return output.Memory;
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        var input = new GetPublishedShoutsReportParam(zone);
        var output = await _axusService.GetPublishedShoutsReportAsync(input, cancellationToken);
        return output.PublishedShouts;
    }

    public async ValueTask PublishShoutAsync(Shout shout, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        var input = new PublishShoutParam(shout, properties.ToArray(), zone);
        await _axusService.PublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishShoutParam(signature, channel, zone);
        await _axusService.UnpublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        var input = new GetSubscribedShoutsReportParam(zone);
        var output = await _axusService.GetSubscribedShoutsReportAsync(input, cancellationToken);
        return output.SubscribedShouts;
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeShoutParam(signature, channel, properties.ToArray(), zone);
        await _axusService.SubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeShoutParam(signature, channel, zone);
        await _axusService.UnsubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, string channel, Timestamp64 createdTime, CancellationToken cancellationToken = default)
    {
        var input = new TryExportShoutParam(signature, channel, createdTime);
        var output = await _axusService.TryExportShoutAsync(input, cancellationToken);
        return output.Shout;
    }
}
