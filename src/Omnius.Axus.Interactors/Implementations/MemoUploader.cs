using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class MemoUploader : AsyncDisposableBase, IMemoUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly MemoUploaderOptions _options;

    private readonly NoteUploaderRepository _barkUploaderRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "bark/v1";
    private const string Zone = "bark-uploader-v1";

    public static async ValueTask<MemoUploader> CreateAsync(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, MemoUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var barkUploader = new MemoUploader(serviceMediator, singleValueStorageFactory, bytesPool, options);
        await barkUploader.InitAsync(cancellationToken);
        return barkUploader;
    }

    private MemoUploader(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, MemoUploaderOptions options)
    {
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _barkUploaderRepo = new NoteUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

        _barkUploaderRepo.Dispose();
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

                await this.SyncMemoUploaderRepo(config, cancellationToken);
                await this.ShrinkPublishedShouts(cancellationToken);
                await this.ShrinkPublishedFiles(cancellationToken);

                if (!await this.ExistsPublishedShouts(cancellationToken) || !await this.ExistsPublishedFiles(cancellationToken))
                {
                    await this.PublishBarkContent(config, cancellationToken);
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

    private async ValueTask SyncMemoUploaderRepo(MemoUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _barkUploaderRepo.BarkItems.FindAll())
            {
                if (profileItem.Signature == config.DigitalSignature.GetOmniSignature()) continue;
                _barkUploaderRepo.BarkItems.Delete(profileItem.Signature);
            }

            if (_barkUploaderRepo.BarkItems.Exists(config.DigitalSignature.GetOmniSignature())) return;

            var newBarkItem = new NoteUploadingItem()
            {
                Signature = config.DigitalSignature.GetOmniSignature(),
            };
            _barkUploaderRepo.BarkItems.Upsert(newBarkItem);
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
                if (_barkUploaderRepo.BarkItems.Exists(signature)) continue;
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
                if (_barkUploaderRepo.BarkItems.Exists(rootHash)) continue;
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

            foreach (var profileItem in _barkUploaderRepo.BarkItems.FindAll())
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
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(Zone, cancellationToken);
            var rootHashes = reports.Select(n => n.RootHash).WhereNotNull().ToHashSet();

            foreach (var profileItem in _barkUploaderRepo.BarkItems.FindAll())
            {
                if (rootHashes.Contains(profileItem.RootHash)) continue;
                return false;
            }

            return true;
        }
    }

    private async ValueTask PublishBarkContent(MemoUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var digitalSignature = config.DigitalSignature;
            var content = new NoteContent(config.Memos.ToArray());

            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(contentBytes.Memory, 8 * 1024 * 1024, Zone, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Zone, cancellationToken);
        }
    }

    public async ValueTask<MemoUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<MemoUploaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new MemoUploaderConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256),
                    memos: Array.Empty<Memo>()
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(MemoUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _barkUploaderRepo.BarkItems.DeleteAll();
        }
    }
}
