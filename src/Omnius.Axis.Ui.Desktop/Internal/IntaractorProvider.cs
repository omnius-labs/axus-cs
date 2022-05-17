using Omnius.Axis.Intaractors;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Storages;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface IIntaractorProvider : IAsyncDisposable
{
    ValueTask<IServiceController> GetServiceControllerAsync(CancellationToken cancellationToken = default);

    ValueTask<IFileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default);

    ValueTask<IFileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default);

    ValueTask<IProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default);

    ValueTask<IProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default);
}

public partial class IntaractorProvider : AsyncDisposableBase, IIntaractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string _databaseDirectoryPath;
    private OmniAddress _listenAddress;
    private readonly IBytesPool _bytesPool;

    private ServiceManager? _serviceManager;

    private ServiceController? _serviceController;
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
        await this.StopAsync();
    }

    public async ValueTask<IServiceController> GetServiceControllerAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _serviceController!;
        }
    }

    public async ValueTask<IFileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileDownloader!;
        }
    }

    public async ValueTask<IFileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileUploader!;
        }
    }

    public async ValueTask<IProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _profilePublisher!;
        }
    }

    public async ValueTask<IProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default)
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
            cancellationToken.ThrowIfCancellationRequested();

            if (_serviceManager is not null && _serviceManager.IsConnected) return;

            _logger.Debug("Update Start: ServiceManager");

            try
            {
                await this.StopAsync(cancellationToken);
                await this.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Disconnected is Service");
            }

            _logger.Debug("Update End: ServiceManager");

            await Task.Delay(1000 * 3);
        }
    }

    private async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _serviceManager = await ServiceManager.CreateAsync(_listenAddress, cancellationToken);
        var service = _serviceManager.GetService()!;
        _serviceController = new ServiceController(service);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_databaseDirectoryPath, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(_serviceController, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(_serviceController, SingleValueFileStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

        var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_databaseDirectoryPath, "profile_publisher"));
        _profilePublisher = await ProfilePublisher.CreateAsync(_serviceController, SingleValueFileStorage.Factory, _bytesPool, profilePublisherOptions, cancellationToken);

        var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_databaseDirectoryPath, "profile_subscriber"));
        _profileSubscriber = await ProfileSubscriber.CreateAsync(_serviceController, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions, cancellationToken);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_profilePublisher is not null) await _profilePublisher.DisposeAsync();
        _profilePublisher = null;

        if (_profileSubscriber is not null) await _profileSubscriber.DisposeAsync();
        _profileSubscriber = null;

        if (_fileUploader is not null) await _fileUploader.DisposeAsync();
        _fileUploader = null;

        if (_fileDownloader is not null) await _fileDownloader.DisposeAsync();
        _fileDownloader = null;

        if (_serviceManager is not null) await _serviceManager.DisposeAsync();
        _serviceManager = null;
        _serviceController = null;
    }
}
