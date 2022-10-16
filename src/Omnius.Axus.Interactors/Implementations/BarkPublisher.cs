using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class BarkPublisher : AsyncDisposableBase, IBarkPublisher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly BarkPublisherOptions _options;

    private readonly BarkPublisherRepository _barkPublisherRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "profile/v1";
    private const string Author = "profile_publisher/v1";

    public static async ValueTask<BarkPublisher> CreateAsync(IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, BarkPublisherOptions options, CancellationToken cancellationToken = default)
    {
        var profilePublisher = new BarkPublisher(serviceMediator, singleValueStorageFactory, bytesPool, options);
        await profilePublisher.InitAsync(cancellationToken);
        return profilePublisher;
    }

    private BarkPublisher(IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, BarkPublisherOptions options)
    {
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _barkPublisherRepo = new BarkPublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

        _barkPublisherRepo.Dispose();
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

                await this.SyncBarkPublisherRepo(config, cancellationToken);
                await this.ShrinkPublishedShouts(cancellationToken);
                await this.ShrinkPublishedFiles(cancellationToken);

                bool exists = await this.ExistsPublishedShouts(cancellationToken);
                if (exists) exists = await this.ExistsPublishedFiles(cancellationToken);

                if (!exists) await this.PublishBarkContent(config, cancellationToken);
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

    private async ValueTask SyncBarkPublisherRepo(BarkPublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _barkPublisherRepo.BarkItems.FindAll())
            {
                if (profileItem.Signature == config.DigitalSignature.GetOmniSignature()) continue;
                _barkPublisherRepo.BarkItems.Delete(profileItem.Signature);
            }

            if (_barkPublisherRepo.BarkItems.Exists(config.DigitalSignature.GetOmniSignature())) return;

            var newBarkItem = new PublishedBarkItem()
            {
                Signature = config.DigitalSignature.GetOmniSignature(),
            };
            _barkPublisherRepo.BarkItems.Upsert(newBarkItem);
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
                if (_barkPublisherRepo.BarkItems.Exists(signature)) continue;
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
                if (_barkPublisherRepo.BarkItems.Exists(rootHash)) continue;
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

            foreach (var profileItem in _barkPublisherRepo.BarkItems.FindAll())
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

            foreach (var profileItem in _barkPublisherRepo.BarkItems.FindAll())
            {
                if (rootHashes.Contains(profileItem.RootHash)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask PublishBarkContent(BarkPublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var digitalSignature = config.DigitalSignature;
            var content = new BarkContent(config.Messages.ToArray());

            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(contentBytes.Memory, 8 * 1024 * 1024, Author, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Author, cancellationToken);
        }
    }

    public async ValueTask<BarkPublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<BarkPublisherConfig>(cancellationToken);

            if (config is null)
            {
                config = new BarkPublisherConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256),
                    messages: Array.Empty<BarkMessage>()
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(BarkPublisherConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _barkPublisherRepo.BarkItems.DeleteAll();
        }
    }
}
