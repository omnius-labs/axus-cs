using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class NoteUploader : AsyncDisposableBase, INoteUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly NoteUploaderOptions _options;

    private readonly ISingleValueStorage _configStorage;
    private NoteUploaderConfig? _config;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string PROPERTIES_SIGNATURE = "Signature";

    private const string Channel = "note-v1";
    private const string Zone = "note-uploader-v1";

    public static async ValueTask<NoteUploader> CreateAsync(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, NoteUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var noteUploader = new NoteUploader(serviceMediator, singleValueStorageFactory, bytesPool, options);
        await noteUploader.InitAsync(cancellationToken);
        return noteUploader;
    }

    private NoteUploader(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, NoteUploaderOptions options)
    {
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

    private async ValueTask SyncAsync(CancellationToken cancellationToken)
    {
        // 1. 不要なPublishedShoutを削除
        // 2. 不要なPublishedFileを削除
        // 3. 未Publishの場合はNoteBoxをPublish

        bool exists = true;

        if (!exists)
        {

        }
    }

    private async ValueTask<bool> TryRemoveUnusedPublishedShoutsAsync(CancellationToken cancellationToken = default)
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

    private async ValueTask<bool> TryRemoveUnusedPublishedFilesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            bool exists = false;

            var config = await this.GetConfigAsync(cancellationToken);
            var reports = await _serviceMediator.GetPublishedFileReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (report.RootHash is null) continue;
                if (!report.Properties.TryGetValue<OmniSignature>(PROPERTIES_SIGNATURE, out var signature)) continue;

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

    private async ValueTask PublishNoteBoxAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.GetConfigAsync(cancellationToken);
            var digitalSignature = config.DigitalSignature;

            var noteBox = new NoteBox(config.Notes.ToArray());
            using var noteBoxBytes = RocketMessage.ToBytes(noteBox);

            using var signatureBytes = RocketMessage.ToBytes(digitalSignature.GetOmniSignature());
            var property = new AttachedProperty(PROPERTIES_SIGNATURE, signatureBytes.Memory);

            var rootHash = await _serviceMediator.PublishFileFromMemoryAsync(noteBoxBytes.Memory, 8 * 1024 * 1024, new[] { property }, Zone, cancellationToken);

            var now = DateTime.UtcNow;
            using var shout = Shout.Create(Channel, Timestamp64.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceMediator.PublishShoutAsync(shout, Enumerable.Empty<AttachedProperty>(), Zone, cancellationToken);
        }
    }

    public async ValueTask<NoteUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_config is not null) return _config;

            _config = await _configStorage.TryGetValueAsync<NoteUploaderConfig>(cancellationToken);

            if (_config is null)
            {
                _config = new NoteUploaderConfig(
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256),
                    notes: Array.Empty<Note>()
                );

                await _configStorage.TrySetValueAsync(_config, cancellationToken);
            }

            return _config;
        }
    }

    public async ValueTask SetConfigAsync(NoteUploaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _config = config;
        }
    }
}
