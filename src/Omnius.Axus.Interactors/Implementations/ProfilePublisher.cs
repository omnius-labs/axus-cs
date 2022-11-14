using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class ProfilePublisher : AsyncDisposableBase, IProfilePublisher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly ProfilePublisherOptions _options;

    private readonly ProfilePublisherRepository _profilePublisherRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "profile/v1";
    private const string Author = "profile_publisher/v1";

    public static async ValueTask<ProfilePublisher> CreateAsync(IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options, CancellationToken cancellationToken = default)
    {
        var profilePublisher = new ProfilePublisher(serviceMediator, singleValueStorageFactory, bytesPool, options);
        await profilePublisher.InitAsync(cancellationToken);
        return profilePublisher;
    }

    private ProfilePublisher(IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options)
    {
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _profilePublisherRepo = new ProfilePublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

        _profilePublisherRepo.Dispose();
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

                await this.SyncProfilePublisherRepo(config, cancellationToken);
                await this.ShrinkPublishedShouts(cancellationToken);
                await this.ShrinkPublishedFiles(cancellationToken);

                if (!await this.ExistsPublishedShouts(cancellationToken) || !await this.ExistsPublishedFiles(cancellationToken))
                {
                    await this.PublishProfileContent(config, cancellationToken);
                }
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

    private async ValueTask SyncProfilePublisherRepo(ProfilePublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _profilePublisherRepo.ProfileItems.FindAll())
            {
                if (profileItem.Signature == config.DigitalSignature.GetOmniSignature()) continue;
                _profilePublisherRepo.ProfileItems.Delete(profileItem.Signature);
            }

            if (_profilePublisherRepo.ProfileItems.Exists(config.DigitalSignature.GetOmniSignature())) return;

            var newProfileItem = new PublishedProfileItem()
            {
                Signature = config.DigitalSignature.GetOmniSignature(),
            };
            _profilePublisherRepo.ProfileItems.Upsert(newProfileItem);
        }
    }

    private async ValueTask ShrinkPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var signature in signatures)
            {
                if (_profilePublisherRepo.ProfileItems.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Author, cancellationToken);
            }
        }
    }

    private async ValueTask ShrinkPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(cancellationToken);
            var rootHashes = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.RootHash)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_profilePublisherRepo.ProfileItems.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Author, cancellationToken);
            }
        }
    }

    private async ValueTask<bool> ExistsPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var profileItem in _profilePublisherRepo.ProfileItems.FindAll())
            {
                if (signatures.Contains(profileItem.Signature)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask<bool> ExistsPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(cancellationToken);
            var rootHashes = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.RootHash)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToHashSet();

            foreach (var profileItem in _profilePublisherRepo.ProfileItems.FindAll())
            {
                if (rootHashes.Contains(profileItem.RootHash)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask PublishProfileContent(ProfilePublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var digitalSignature = config.DigitalSignature;
            var content = new ProfileContent(config.TrustedSignatures.ToArray(), config.BlockedSignatures.ToArray());

            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(contentBytes.Memory, 8 * 1024 * 1024, Author, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Author, cancellationToken);
        }
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
            _profilePublisherRepo.ProfileItems.DeleteAll();
        }
    }
}
