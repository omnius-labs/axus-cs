using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class SeedUploader : AsyncDisposableBase, ISeedUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IFileUploader _fileUploader;
    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly SeedUploaderOptions _options;

    private readonly SeedUploaderRepository _seedUploaderRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "seed/v1";
    private const string Zone = "seed-uploader-v1";

    public static async ValueTask<SeedUploader> CreateAsync(IFileUploader fileUploader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, SeedUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var seedUploader = new SeedUploader(fileUploader, serviceMediator, singleValueStorageFactory, bytesPool, options);
        await seedUploader.InitAsync(cancellationToken);
        return seedUploader;
    }

    private SeedUploader(IFileUploader fileUploader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, SeedUploaderOptions options)
    {
        _fileUploader = fileUploader;
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _seedUploaderRepo = new SeedUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

        _seedUploaderRepo.Dispose();
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

                await this.SyncSeedUploaderRepo(config, cancellationToken);
                await this.ShrinkPublishedShouts(cancellationToken);
                await this.ShrinkPublishedFiles(cancellationToken);

                if (!await this.ExistsPublishedShouts(cancellationToken) || !await this.ExistsPublishedFiles(cancellationToken))
                {
                    await this.PublishContent(config, cancellationToken);
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

    private async ValueTask SyncSeedUploaderRepo(SeedUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var item in _seedUploaderRepo.Items.FindAll())
            {
                if (item.Signature == config.DigitalSignature.GetOmniSignature()) continue;
                _seedUploaderRepo.Items.Delete(item.Signature);
            }

            if (_seedUploaderRepo.Items.Exists(config.DigitalSignature.GetOmniSignature())) return;

            var newItem = new CaskUploadingItem()
            {
                Signature = config.DigitalSignature.GetOmniSignature(),
            };
            _seedUploaderRepo.Items.Upsert(newItem);
        }
    }

    private async ValueTask ShrinkPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedShoutReportsAsync(Zone, cancellationToken);
            var signatures = reports.Select(n => n.Signature).ToHashSet();

            foreach (var signature in signatures)
            {
                if (_seedUploaderRepo.Items.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Zone, cancellationToken);
            }
        }
    }

    private async ValueTask ShrinkPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(Zone, cancellationToken);
            var rootHashes = reports.Select(n => n.RootHash).WhereNotNull().ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_seedUploaderRepo.Items.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Zone, cancellationToken);
            }
        }
    }

    private async ValueTask<bool> ExistsPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedShoutReportsAsync(Zone, cancellationToken);
            var signatures = reports.Select(n => n.Signature).ToHashSet();

            foreach (var item in _seedUploaderRepo.Items.FindAll())
            {
                if (signatures.Contains(item.Signature)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask<bool> ExistsPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(Zone, cancellationToken);
            var rootHashes = reports.Select(n => n.RootHash).WhereNotNull().ToHashSet();

            foreach (var item in _seedUploaderRepo.Items.FindAll())
            {
                if (rootHashes.Contains(item.RootHash)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask PublishContent(SeedUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _fileUploader.GetUploadingFileReportsAsync(cancellationToken);
            var seeds = reports.Select(n => n.Seed).WhereNotNull().ToArray();

            var digitalSignature = config.DigitalSignature;
            var content = new CaskContent(seeds);

            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(contentBytes.Memory, 8 * 1024 * 1024, Zone, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Zone, cancellationToken);
        }
    }

    public async ValueTask<SeedUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<SeedUploaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new SeedUploaderConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256)
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(SeedUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _seedUploaderRepo.Items.DeleteAll();
        }
    }
}
