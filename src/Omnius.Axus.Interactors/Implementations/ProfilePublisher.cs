using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class ProfilePublisher : AsyncDisposableBase, IProfilePublisher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly ProfilePublisherOptions _options;

    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "profile/v1";
    private const string Registrant = "profile_publisher/v1";

    public static async ValueTask<ProfilePublisher> CreateAsync(IServiceMediator serviceController, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options, CancellationToken cancellationToken = default)
    {
        var profilePublisher = new ProfilePublisher(serviceController, singleValueStorageFactory, bytesPool, options);
        await profilePublisher.InitAsync(cancellationToken);
        return profilePublisher;
    }

    private ProfilePublisher(IServiceMediator serviceController, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options)
    {
        _serviceController = serviceController;
        _bytesPool = bytesPool;
        _options = options;

        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _configStorage.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken).ConfigureAwait(false);

                var config = await this.GetConfigAsync(cancellationToken);

                if (await this.ExistsProfile(config, cancellationToken)) continue;

                await this.RemoveProfile(cancellationToken);
                await this.PublishProfile(config, cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<bool> ExistsProfile(ProfilePublisherConfig config, CancellationToken cancellationToken = default)
    {
        var rootHash = await this.FetchShout(config.DigitalSignature.GetOmniSignature(), cancellationToken);
        if (rootHash is null) return false;

        var content = await this.FetchFile(rootHash.Value, cancellationToken);
        if (content is null) return false;

        var result =
            CollectionHelper.Equals(config.TrustedSignatures, content.TrustedSignatures)
            && CollectionHelper.Equals(config.BlockedSignatures, content.BlockedSignatures);

        return result;
    }

    private async ValueTask<OmniHash?> FetchShout(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var shout = await _serviceController.TryExportShoutAsync(signature, Channel);
            if (shout is null) return null;

            var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
            return rootHash;
        }
    }

    private async ValueTask<ProfileContent?> FetchFile(OmniHash rootHash, CancellationToken cancellationToken)
    {
        using var contentBytes = await _serviceController.TryExportFileToMemoryAsync(rootHash, cancellationToken);
        if (contentBytes is null) return null;

        var content = RocketMessage.FromBytes<ProfileContent>(contentBytes.Memory);
        return content;
    }

    private async ValueTask RemoveProfile(CancellationToken cancellationToken = default)
    {
        await this.RemoveShout(cancellationToken);
        await this.RemoveFile(cancellationToken);
    }

    private async ValueTask RemoveShout(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceController.GetPublishedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Registrant == Registrant)
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var signature in signatures)
            {
                await _serviceController.UnpublishShoutAsync(signature, Channel, Registrant, cancellationToken);
            }
        }
    }

    private async ValueTask RemoveFile(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceController.GetPublishedFileReportsAsync(cancellationToken);
            var rootHashes = reports
                .Where(n => n.Registrant == Registrant)
                .Select(n => n.RootHash)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                await _serviceController.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
            }
        }
    }

    private async ValueTask PublishProfile(ProfilePublisherConfig config, CancellationToken cancellationToken = default)
    {
        var digitalSignature = config.DigitalSignature;
        var content = new ProfileContent(config.TrustedSignatures.ToArray(), config.BlockedSignatures.ToArray());

        var rootHash = await this.PublishFile(content, cancellationToken);
        await this.PublishShout(rootHash, digitalSignature, cancellationToken);
    }

    private async ValueTask<OmniHash> PublishFile(ProfileContent content, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _serviceController.PublishFileFromMemoryAsync(contentBytes.Memory, Registrant, cancellationToken);
            return rootHash;
        }
    }

    private async ValueTask PublishShout(OmniHash rootHash, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
        await _serviceController.PublishShoutAsync(shout, Registrant, cancellationToken);
    }

    public async ValueTask<ProfilePublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<ProfilePublisherConfig>(cancellationToken);

            if (config is null)
            {
                config = new ProfilePublisherConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256),
                    trustedSignatures: Array.Empty<OmniSignature>(),
                    blockedSignatures: Array.Empty<OmniSignature>()
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(ProfilePublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
