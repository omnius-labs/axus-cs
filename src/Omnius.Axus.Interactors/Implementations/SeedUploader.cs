using System.Buffers;
using Omnius.Axus.Interactors.Internal.Models;
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

    private readonly ISingleValueStorage _configStorage;
    private SeedUploaderConfig? _config;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "seed-box-v1";
    private const string Zone = "seed-box-uploader-v1";

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

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        // 1. 不要なPublishedShoutsを削除
        // 2. 不要なPublishedFilesを削除
        // 3. 未Publishの場合はSeedBoxをPublish

        bool exists = true;
        exists &= await this.TrySyncPublishedShoutsAsync(cancellationToken);
        exists &= await this.TrySyncPublishedFilesAsync(cancellationToken);

        if (!exists)
        {
            await this.PublishSeedBoxAsync(cancellationToken);
        }
    }

    private async ValueTask<bool> TrySyncPublishedShoutsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            bool exists = false;

            var config = await this.GetConfigAsync(cancellationToken);
            var reports = await _serviceMediator.GetPublishedShoutReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (report.Signature == config.DigitalSignature.GetOmniSignature())
                {
                    exists = true;
                    continue;
                }

                await _serviceMediator.UnsubscribeShoutAsync(report.Signature, Channel, Zone, cancellationToken);
            }

            return exists;
        }
    }

    private async ValueTask<bool> TrySyncPublishedFilesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            bool exists = false;

            var config = await this.GetConfigAsync(cancellationToken);
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (report.RootHash is null) continue;

                if (!report.Properties.TryGetValue("Signature", out var signatureBytes)) continue;
                var signature = RocketMessage.FromBytes<OmniSignature>(signatureBytes);

                if (signature == config.DigitalSignature.GetOmniSignature())
                {
                    exists = true;
                    continue;
                }

                await _serviceMediator.UnpublishFileFromMemoryAsync(report.RootHash.Value, Zone, cancellationToken);
            }

            return exists;
        }
    }

    private async ValueTask PublishSeedBoxAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.GetConfigAsync(cancellationToken);
            var digitalSignature = config.DigitalSignature;

            var reports = await _fileUploader.GetUploadingFileReportsAsync(cancellationToken);
            var seeds = reports.Select(n => n.Seed).WhereNotNull().ToArray();

            var content = new SeedBox(seeds);
            using var contentBytes = RocketMessage.ToBytes(content);

            using var signatureBytes = RocketMessage.ToBytes(digitalSignature.GetOmniSignature());
            var property = new AttachedProperty("Signature", signatureBytes.Memory);

            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(contentBytes.Memory, 8 * 1024 * 1024, new[] { property }, Zone, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Enumerable.Empty<AttachedProperty>(), Zone, cancellationToken);
        }
    }

    public async ValueTask<SeedUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_config is not null) return _config;

            _config = await _configStorage.TryGetValueAsync<SeedUploaderConfig>(cancellationToken);

            if (_config is null)
            {
                _config = new SeedUploaderConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256)
                );

                await _configStorage.TrySetValueAsync(_config, cancellationToken);
            }

            return _config;
        }
    }

    public async ValueTask SetConfigAsync(SeedUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _config = config;
        }
    }
}
