
using System.Buffers;
using Omnius.Axus.Engine.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Engine;

public sealed class AxusEngine
{
    public async ValueTask<EngineConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask SetConfigAsync(EngineConfig config, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<SessionReport> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFilesAsync(GetPublishedFilesMatcher matcher, GetPublishedFilesDirection direction, long offset, long limit, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<OmniHash> PublishFileAsync(string filePath, int maxBlockSize, AttachedProperty? property, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask UnpublishFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFilesAsync(GetSubscribedFilesMatcher matcher, GetSubscribedFilesDirection direction, long offset, long limit, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, AttachedProperty? property, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask PublishShoutAsync(Shout shout, AttachedProperty? property, string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, AttachedProperty? property, string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<Shout?> TryExportShoutAsync(OmniSignature signature, string channel, Timestamp64 createdTime, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
