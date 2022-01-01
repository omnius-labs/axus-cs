using System.Buffers;
using Omnius.Axis.Models;
using Omnius.Axis.Remoting;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors;

public sealed class ServiceAdapter : IServiceAdapter
{
    private readonly IAxisService _axisService;

    public ServiceAdapter(IAxisService axisService)
    {
        _axisService = axisService;
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetSessionsReportAsync(cancellationToken);
        return output.Sessions;
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetMyNodeLocationAsync(cancellationToken);
        return output.NodeLocation;
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        var input = new AddCloudNodeLocationsRequest(nodeLocations.ToArray());
        await _axisService.AddCloudNodeLocationsAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetPublishedFilesReportAsync(cancellationToken);
        return output.PublishedFiles;
    }

    public async ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromStorageRequest(filePath, registrant);
        var output = await _axisService.PublishFileFromStorageAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromMemoryRequest(memory, registrant);
        var output = await _axisService.PublishFileFromMemoryAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask UnpublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromStorageRequest(filePath, registrant);
        await _axisService.UnpublishFileFromStorageAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromMemoryRequest(rootHash, registrant);
        await _axisService.UnpublishFileFromMemoryAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetSubscribedFilesReportAsync(cancellationToken);
        return output.SubscribedFiles;
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeFileRequest(rootHash, registrant);
        await _axisService.SubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeFileRequest(rootHash, registrant);
        await _axisService.UnsubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToStorageRequest(rootHash, filePath);
        var output = await _axisService.TryExportFileToStorageAsync(input, cancellationToken);
        return output.Success;
    }

    public async ValueTask<ReadOnlyMemory<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToMemoryRequest(rootHash);
        var output = await _axisService.TryExportFileToMemoryAsync(input, cancellationToken);
        return output.Memory;
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetPublishedShoutsReportAsync(cancellationToken);
        return output.PublishedShouts;
    }

    public async ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishShoutRequest(message, registrant);
        await _axisService.PublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishShoutRequest(signature, registrant);
        await _axisService.UnpublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _axisService.GetSubscribedShoutsReportAsync(cancellationToken);
        return output.SubscribedShouts;
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeShoutRequest(signature, registrant);
        await _axisService.SubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeShoutRequest(signature, registrant);
        await _axisService.UnsubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        var input = new TryExportShoutRequest(signature);
        var output = await _axisService.TryExportShoutAsync(input, cancellationToken);
        return output.Shout;
    }
}
