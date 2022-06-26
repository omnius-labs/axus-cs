using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Intaractors.Internal.Repositories;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axis.Intaractors;

public sealed class ProfilePublisher : AsyncDisposableBase, IProfilePublisher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxisServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly ProfilePublisherOptions _options;

    private readonly ProfilePublisherRepository _profilePublisherRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Registrant = "Omnius.Axis.Intaractors.ProfilePublisher";

    public static async ValueTask<ProfilePublisher> CreateAsync(IAxisServiceMediator serviceController, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options, CancellationToken cancellationToken = default)
    {
        var profilePublisher = new ProfilePublisher(serviceController, singleValueStorageFactory, bytesPool, options);
        await profilePublisher.InitAsync(cancellationToken);
        return profilePublisher;
    }

    private ProfilePublisher(IAxisServiceMediator serviceController, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, ProfilePublisherOptions options)
    {
        _serviceController = serviceController;
        _bytesPool = bytesPool;
        _options = options;

        _profilePublisherRepo = new ProfilePublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
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
        _configStorage.Dispose();
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
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async Task SyncPublishedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var publishedShoutReports = await _serviceController.GetPublishedShoutReportsAsync(cancellationToken);
            var signatures = new HashSet<OmniSignature>();
            signatures.UnionWith(publishedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

            foreach (var signature in signatures)
            {
                if (_profilePublisherRepo.Items.Exists(signature)) continue;
                await _serviceController.UnpublishShoutAsync(signature, Registrant, cancellationToken);
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
            var publishedFileReports = await _serviceController.GetPublishedFileReportsAsync(cancellationToken);
            var rootHashes = new HashSet<OmniHash>();
            rootHashes.UnionWith(publishedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

            foreach (var rootHash in rootHashes)
            {
                if (_profilePublisherRepo.Items.Exists(rootHash)) continue;
                await _serviceController.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
            }

            foreach (var rootHash in _profilePublisherRepo.Items.FindAll().Select(n => n.RootHash))
            {
                if (rootHashes.Contains(rootHash)) continue;
                _profilePublisherRepo.Items.Delete(rootHash);
            }
        }
    }

    public async ValueTask PublishAsync(ProfileContent profileContent, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var now = DateTime.UtcNow;

            var config = await this.GetConfigAsync(cancellationToken);
            var digitalSignature = config.DigitalSignature;

            using var profileContentBytes = RocketMessage.ToBytes(profileContent);

            var rootHash = await _serviceController.PublishFileFromMemoryAsync(profileContentBytes.Memory, Registrant, cancellationToken);
            var shout = Shout.Create(Timestamp.FromDateTime(now), RocketMessage.ToBytes(rootHash), digitalSignature);
            await _serviceController.PublishShoutAsync(shout, Registrant, cancellationToken);
            shout.Value.Dispose();

            var item = new PublishedProfileItem(digitalSignature.GetOmniSignature(), rootHash, now);
            _profilePublisherRepo.Items.DeleteAll();
            _profilePublisherRepo.Items.Upsert(item);
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
                    digitalSignature: OmniDigitalSignature.Create("Anonymous", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256)
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
