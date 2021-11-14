using System.Buffers;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Models;
using Omnius.Xeus.Remoting;

namespace Omnius.Xeus.Intaractors.Internal;

internal sealed class XeusServiceAdapter
{
    private readonly IXeusService _xeusService;

    public XeusServiceAdapter(IXeusService xeusService)
    {
        _xeusService = xeusService;
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetSessionsReportAsync(cancellationToken);
        return output.Sessions;
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetMyNodeLocationAsync(cancellationToken);
        return output.NodeLocation;
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        var input = new AddCloudNodeLocationsRequest(nodeLocations.ToArray());
        await _xeusService.AddCloudNodeLocationsAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetPublishedFilesReportAsync(cancellationToken);
        return output.PublishedFiles;
    }

    public async ValueTask<OmniHash> PublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromStorageRequest(filePath, registrant);
        var output = await _xeusService.PublishFileFromStorageAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask<OmniHash> PublishFileFromMemoryAsync(ReadOnlyMemory<byte> memory, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishFileFromMemoryRequest(memory, registrant);
        var output = await _xeusService.PublishFileFromMemoryAsync(input, cancellationToken);
        return output.Hash;
    }

    public async ValueTask UnpublishFileFromStorageAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromStorageRequest(filePath, registrant);
        await _xeusService.UnpublishFileFromStorageAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishFileFromMemoryAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishFileFromMemoryRequest(rootHash, registrant);
        await _xeusService.UnpublishFileFromMemoryAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetSubscribedFilesReportAsync(cancellationToken);
        return output.SubscribedFiles;
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeFileRequest(rootHash, registrant);
        await _xeusService.SubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeFileRequest(rootHash, registrant);
        await _xeusService.UnsubscribeFileAsync(input, cancellationToken);
    }

    public async ValueTask<bool> TryExportFileToStorageAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToStorageRequest(rootHash, filePath);
        var output = await _xeusService.TryExportFileToStorageAsync(input, cancellationToken);
        return output.Success;
    }

    public async ValueTask<ReadOnlyMemory<byte>?> TryExportFileToMemoryAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        var input = new TryExportFileToMemoryRequest(rootHash);
        var output = await _xeusService.TryExportFileToMemoryAsync(input, cancellationToken);
        return output.Memory;
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetPublishedShoutsReportAsync(cancellationToken);
        return output.PublishedShouts;
    }

    public async ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new PublishShoutRequest(message, registrant);
        await _xeusService.PublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnpublishShoutRequest(signature, registrant);
        await _xeusService.UnpublishShoutAsync(input, cancellationToken);
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        var output = await _xeusService.GetSubscribedShoutsReportAsync(cancellationToken);
        return output.SubscribedShouts;
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new SubscribeShoutRequest(signature, registrant);
        await _xeusService.SubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        var input = new UnsubscribeShoutRequest(signature, registrant);
        await _xeusService.UnsubscribeShoutAsync(input, cancellationToken);
    }

    public async ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        var input = new TryExportShoutRequest(signature);
        var output = await _xeusService.TryExportShoutAsync(input, cancellationToken);
        return output.Shout;
    }
}
