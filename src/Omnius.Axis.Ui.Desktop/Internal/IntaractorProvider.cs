using Omnius.Axis.Intaractors;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Storages;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface IIntaractorProvider
{
    ValueTask<Dashboard> GetDashboardAsync(CancellationToken cancellationToken = default);

    ValueTask<FileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default);

    ValueTask<FileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default);

    ValueTask<ProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default);

    ValueTask<ProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default);
}

public partial class IntaractorProvider : AsyncDisposableBase, IIntaractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string _databaseDirectoryPath;
    private OmniAddress _listenAddress;
    private readonly IBytesPool _bytesPool;

    private ServiceManager? _serviceManager;

    private Dashboard? _dashboard;
    private FileDownloader? _fileDownloader;
    private FileUploader? _fileUploader;
    private ProfilePublisher? _profilePublisher;
    private ProfileSubscriber? _profileSubscriber;

    private readonly AsyncLock _asyncLock = new();

    public IntaractorProvider(string databaseDirectoryPath, OmniAddress listenAddress, IBytesPool bytesPool)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        _listenAddress = listenAddress;
        _bytesPool = bytesPool;
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_serviceManager is not null) await _serviceManager.DisposeAsync();
    }

    public async ValueTask<Dashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _dashboard!;
        }
    }

    public async ValueTask<FileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileDownloader!;
        }
    }

    public async ValueTask<FileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileUploader!;
        }
    }

    public async ValueTask<ProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _profilePublisher!;
        }
    }

    public async ValueTask<ProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _profileSubscriber!;
        }
    }

    private async ValueTask UpdateAsync(CancellationToken cancellationToken = default)
    {
        for (; ; )
        {
            if (_serviceManager is not null && _serviceManager.IsConnected)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await this.StopAsync(cancellationToken);
                await this.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }

            await Task.Delay(3000);
        }
    }

    private async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _serviceManager = await ServiceManager.CreateAsync(_listenAddress, cancellationToken);
        var service = _serviceManager.GetService()!;

        _dashboard = await Dashboard.CreateAsync(service, _bytesPool, cancellationToken);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_databaseDirectoryPath, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

        var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_databaseDirectoryPath, "profile_publisher"));
        _profilePublisher = await ProfilePublisher.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profilePublisherOptions);

        var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_databaseDirectoryPath, "profile_subscriber"));
        _profileSubscriber = await ProfileSubscriber.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_dashboard is not null) await _dashboard.DisposeAsync();
        if (_fileUploader is not null) await _fileUploader.DisposeAsync();
        if (_fileDownloader is not null) await _fileDownloader.DisposeAsync();
        if (_profilePublisher is not null) await _profilePublisher.DisposeAsync();
        if (_profileSubscriber is not null) await _profileSubscriber.DisposeAsync();
        if (_serviceManager is not null) await _serviceManager.DisposeAsync();
    }
}
