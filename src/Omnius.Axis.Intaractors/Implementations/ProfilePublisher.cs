using Omnius.Axis.Intaractors.Internal;
using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Intaractors.Internal.Repositories;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Models;
using Omnius.Axis.Remoting;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axis.Intaractors;

public sealed class ProfilePublisher : AsyncDisposableBase, IProfilePublisher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly AxisServiceAdapter _service;
    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly ProfilePublisherOptions _options;

    private readonly ProfilePublisherRepository _profilePublisherRepo;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Registrant = "Omnius.Axis.Intaractors.ProfilePublisher";

    public static async ValueTask<ProfilePublisher> CreateAsync(IAxisService axisService, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options, CancellationToken cancellationToken = default)
    {
        var profilePublisher = new ProfilePublisher(axisService, keyValueStorageFactory, bytesPool, options);
        await profilePublisher.InitAsync(cancellationToken);
        return profilePublisher;
    }

    private ProfilePublisher(IAxisService axisService, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options)
    {
        _service = new AxisServiceAdapter(axisService);
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _profilePublisherRepo = new ProfilePublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _profilePublisherRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _profilePublisherRepo.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(1000 * 30, cancellationToken);

                await this.SyncPublishedShouts(cancellationToken);
                await this.SyncPublishedFiles(cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async Task SyncPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var publishedShoutReports = await _service.GetPublishedShoutReportsAsync(cancellationToken);
            var signatures = new HashSet<OmniSignature>();
            signatures.UnionWith(publishedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

            foreach (var signature in signatures)
            {
                if (_profilePublisherRepo.Items.Exists(signature)) continue;
                await _service.UnpublishShoutAsync(signature, Registrant, cancellationToken);
            }

            foreach (var item in _profilePublisherRepo.Items.FindAll())
            {
                if (signatures.Contains(item.Signature)) continue;
                _profilePublisherRepo.Items.Delete(item.Signature);
            }
        }
    }

    private async Task SyncPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var publishedFileReports = await _service.GetPublishedFileReportsAsync(cancellationToken);
            var rootHashes = new HashSet<OmniHash>();
            rootHashes.UnionWith(publishedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

            foreach (var rootHash in rootHashes)
            {
                if (_profilePublisherRepo.Items.Exists(rootHash)) continue;
                await _service.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
            }

            foreach (var rootHash in _profilePublisherRepo.Items.FindAll().Select(n => n.RootHash))
            {
                if (rootHashes.Contains(rootHash)) continue;
                _profilePublisherRepo.Items.Delete(rootHash);
            }
        }
    }

    public async ValueTask<IEnumerable<PublishedProfileReport>> GetPublishedProfileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<PublishedProfileReport>();

            foreach (var item in _profilePublisherRepo.Items.FindAll())
            {
                reports.Add(new PublishedProfileReport(item.CreationTime, item.Signature));
            }

            return reports;
        }
    }

    public async ValueTask RegisterAsync(ProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var contentBytes = RocketMessage.ToBytes(content);

            var contentRootHash = await _service.PublishFileFromMemoryAsync(contentBytes.Memory, Registrant, cancellationToken);

            var now = DateTime.UtcNow;

            var shout = Shout.Create(Timestamp.FromDateTime(now), RocketMessage.ToBytes(contentRootHash), digitalSignature);
            await _service.PublishShoutAsync(shout, Registrant, cancellationToken);
            shout.Value.Dispose();

            var item = new PublishedProfileItem(digitalSignature.GetOmniSignature(), contentRootHash, now);
            _profilePublisherRepo.Items.Upsert(item);
        }
    }

    public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _profilePublisherRepo.Items.FindOne(signature);
            if (item is null) return;

            await _service.UnpublishShoutAsync(item.Signature, Registrant, cancellationToken);
            await _service.UnpublishFileFromMemoryAsync(item.RootHash, Registrant, cancellationToken);

            _profilePublisherRepo.Items.Delete(item.Signature);
        }
    }
}
