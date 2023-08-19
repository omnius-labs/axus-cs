using System.Collections.Immutable;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Axus.Messages;

namespace Omnius.Axus.Interactors;

public sealed partial class NoteDownloader : AsyncDisposableBase, INoteDownloader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IProfileDownloader _profileDownloader;
    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly NoteDownloaderOptions _options;

    private readonly CachedNoteBoxRepository _cachedNoteBoxRepo;
    private readonly ISingleValueStorage _configStorage;
    private NoteDownloaderConfig? _config;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string PROPERTIES_SHOUT = "Shout";

    private const string Channel = "note-v1";
    private const string Zone = "note-downloader-v1";

    public static async ValueTask<NoteDownloader> CreateAsync(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, NoteDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var noteDownloader = new NoteDownloader(profileDownloader, serviceMediator, singleValueStorageFactory, bytesPool, options);
        await noteDownloader.InitAsync(cancellationToken);
        return noteDownloader;
    }

    private NoteDownloader(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, NoteDownloaderOptions options)
    {
        _profileDownloader = profileDownloader;
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedNoteBoxRepo = new CachedNoteBoxRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_memos"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _cachedNoteBoxRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        await _configStorage.DisposeAsync();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);

                await this.SyncAsync(cancellationToken);
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

    private async ValueTask SyncAsync(CancellationToken cancellationToken = default)
    {
        // 1. 不要なSubscribedShoutを削除
        // 2. 不要なSubscribedFileを削除
        // 3. 不要なCachedProfileを削除
        // 4. 新しいSubscribedShoutを追加
        // 5. 新しいSubscribedFileを追加
        // 6. 新しいCachedProfileを追加

        var targetSignatures = await this.GetSignaturesAsync(cancellationToken);

        var subscribedShoutKeys = await this.TryRemoveUnusedSubscribedShoutsAsync(targetSignatures, cancellationToken);
    }

    private async ValueTask<ImmutableHashSet<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var builder = ImmutableHashSet.CreateBuilder<OmniSignature>();

            foreach (var signature in await _profileDownloader.GetSignaturesAsync(cancellationToken))
            {
                builder.Add(signature);
            }

            return builder.ToImmutable();
        }
    }

    private async ValueTask<ImmutableDictionary<OmniSignature, Timestamp64>> TryRemoveUnusedSubscribedShoutsAsync(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableDictionary.CreateBuilder<OmniSignature, Timestamp64>();

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (targetSignatures.Contains(report.Signature))
                {
                    builder.Add(report.Signature, report.CreatedTime);
                    continue;
                }

                await _serviceMediator.UnsubscribeShoutAsync(report.Signature, Channel, Zone, cancellationToken);
            }
        }

        return builder.ToImmutable();
    }

    private async ValueTask<ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)>> TryRemoveUnusedSubscribedFilesAsync(ImmutableDictionary<OmniSignature, Timestamp64> subscribedShoutKeys, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableDictionary.CreateBuilder<OmniSignature, (Timestamp64, OmniHash)>();

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (report.Properties.TryGetValue<Shout>(PROPERTIES_SHOUT, out var shout)
                    && shout.Certificate is not null)
                {
                    if (subscribedShoutKeys.TryGetValue(shout.Certificate.GetOmniSignature(), out var targetCreatedTime)
                        && targetCreatedTime == shout.CreatedTime)
                    {
                        builder.Add(shout.Certificate.GetOmniSignature(), (shout.CreatedTime, report.RootHash));
                        continue;
                    }
                }

                await _serviceMediator.UnsubscribeFileAsync(report.RootHash, Zone, cancellationToken);
            }
        }

        return builder.ToImmutable();
    }

    private async ValueTask<ImmutableDictionary<OmniSignature, Timestamp64>> TryRemoveUnusedCachedSeedBoxesAsync(ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signatures = subscribedFileKeys.Select(n => n.Key).ToList();
            await _cachedNoteBoxRepo.Shrink

        }
    }

    private async ValueTask SyncMemoDownloaderRepo(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _memoDownloaderRepo.BarkItems.FindAll())
            {
                if (targetSignatures.Contains(profileItem.Signature)) continue;
                _memoDownloaderRepo.BarkItems.Delete(profileItem.Signature);
            }

            foreach (var signature in targetSignatures)
            {
                if (_memoDownloaderRepo.BarkItems.Exists(signature)) continue;

                var newBarkItem = new NoteBoxDownloadingItem()
                {
                    Signature = signature,
                    ShoutUpdatedTime = DateTime.MinValue,
                };
                _memoDownloaderRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(Zone, cancellationToken);
            var signatures = reports.Select(n => n.Signature).ToHashSet();

            foreach (var signature in signatures)
            {
                if (_memoDownloaderRepo.BarkItems.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Zone, cancellationToken);
            }

            foreach (var memoItem in _memoDownloaderRepo.BarkItems.FindAll())
            {
                if (signatures.Contains(memoItem.Signature)) continue;
                await _serviceMediator.SubscribeShoutAsync(memoItem.Signature, Channel, Zone, cancellationToken);
            }

            foreach (var memoItem in _memoDownloaderRepo.BarkItems.FindAll())
            {
                using var shout = await _serviceMediator.TryExportShoutAsync(memoItem.Signature, Channel, memoItem.ShoutUpdatedTime, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newBarkItem = memoItem with
                {
                    RootHash = rootHash,
                    ShoutUpdatedTime = shout.UpdatedTime.ToDateTime(),
                };
                _memoDownloaderRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(Zone, cancellationToken);
            var rootHashes = reports.Select(n => n.RootHash).ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_memoDownloaderRepo.BarkItems.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Zone, cancellationToken);
            }

            foreach (var rootHash in _memoDownloaderRepo.BarkItems.FindAll().Select(n => n.RootHash))
            {
                if (rootHash == OmniHash.Empty) continue;
                if (rootHashes.Contains(rootHash)) continue;
                await _serviceMediator.SubscribeFileAsync(rootHash, Zone, cancellationToken);
            }
        }
    }

    private async ValueTask UpdateCachedMemosAsync(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var signature in targetSignatures)
            {
                var cachedContentShoutUpdatedTime = _cachedNoteBoxRepo.FetchShoutUpdatedTime(signature);

                var cachedContent = await this.TryInternalExportAsync(signature, cachedContentShoutUpdatedTime, cancellationToken);
                if (cachedContent is null) continue;

                _cachedNoteBoxRepo.UpsertBulk(cachedContent);
            }
        }
    }

    private async ValueTask<CachedNoteBox?> TryInternalExportAsync(OmniSignature signature, DateTime cachedContentShoutUpdatedTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var memoItem = _memoDownloaderRepo.BarkItems.FindOne(signature);
            if (memoItem is null || memoItem.RootHash == OmniHash.Empty || memoItem.ShoutUpdatedTime <= cachedContentShoutUpdatedTime) return null;

            using var contentBytes = await _serviceMediator.TryExportFileToMemoryAsync(memoItem.RootHash, cancellationToken);
            if (contentBytes is null) return null;

            var content = RocketMessage.FromBytes<NoteBox>(contentBytes.Memory);

            var cachedContent = new CachedNoteBox(signature, Timestamp64.FromDateTime(memoItem.ShoutUpdatedTime), content);
            return cachedContent;
        }
    }

    public async ValueTask<IEnumerable<NoteReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = _cachedNoteBoxRepo.FetchMemoByTag(tag);
            return results.Select(n => n.ToReport());
        }
    }

    public async ValueTask<NoteReport?> FindMessageBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var result = _cachedNoteBoxRepo.FetchMemoBySelfHash(selfHash);
            return result?.ToReport();
        }
    }

    public async ValueTask<MemoDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<MemoDownloaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new MemoDownloaderConfig(
                    tags: Array.Empty<Utf8String>(),
                    maxMemoCount: 10000
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(MemoDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }

    ValueTask<NoteDownloaderConfig> INoteDownloader.GetConfigAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public ValueTask SetConfigAsync(NoteDownloaderConfig config, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
