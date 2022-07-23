using System.Buffers;
using Omnius.Core.Cryptography;

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
        var input = new SetConfigRequest(config);
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
        var input = new AddCloudNodeLocationsRequest(nodeLocations.ToArray());
        await _axusService.AddCloudNodeLocationsAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetPublishedFilesReportAsync(cancellationToken);
        return output.PublishedFiles;
    }

    public async ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromStorageRequest(filePath, registrant);
        var output = await _axusService.PublishFileFromStorageAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromMemoryRequest(memory, registrant);
        var output = await _axusService.PublishFileFromMemoryAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask UnpublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromStorageRequest(filePath, registrant);
        await _axusService.UnpublishFileFromStorageAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromMemoryRequest(rootHash, registrant);
        await _axusService.UnpublishFileFromMemoryAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetSubscribedFilesReportAsync(cancellationToken);
        return output.SubscribedFiles;
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeFileRequest(rootHash, registrant);
        await _axusService.SubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeFileRequest(rootHash, registrant);
        await _axusService.UnsubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToStorageRequest(rootHash, filePath);
        var output = await _axusService.TryExportFileToStorageAsync(input, cancellationToken);
        return output.Success;
    }

    public async ValueTask<IMemoryOwner<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToMemoryRequest(rootHash);
        var output = await _axusService.TryExportFileToMemoryAsync(input, cancellationToken);
        return output.Memory;
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetPublishedShoutsReportAsync(cancellationToken);
        return output.PublishedShouts;
    }

    public async ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishShoutRequest(message, registrant);
        await _axusService.PublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishShoutRequest(signature, registrant);
        await _axusService.UnpublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axusService.GetSubscribedShoutsReportAsync(cancellationToken);
        return output.SubscribedShouts;
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeShoutRequest(signature, registrant);
        await _axusService.SubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeShoutRequest(signature, registrant);
        await _axusService.UnsubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        var input = new TryExportShoutRequest(signature);
        var output = await _axusService.TryExportShoutAsync(input, cancellationToken);
        return output.Shout;
    }
}
