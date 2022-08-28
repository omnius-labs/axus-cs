using Omnius.Axus.Interactors;
using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Axus.Ui.Desktop.Internal;

public interface IInteractorProvider : IAsyncDisposable
{
    IFileDownloader GetFileDownloader();

    IFileUploader GetFileUploader();

    IProfilePublisher GetProfilePublisher();

    IProfileSubscriber GetProfileSubscriber();
}

public partial class InteractorProvider : AsyncDisposableBase, IInteractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string _databaseDirectoryPath;
    private IServiceMediator? _axusServiceMediator;
    private readonly IBytesPool _bytesPool;

    private IInitialNodesFetcher? _initialNodesFetcher;
    private IFileDownloader? _fileDownloader;
    private IFileUploader? _fileUploader;
    private IProfilePublisher? _profilePublisher;
    private IProfileSubscriber? _profileSubscriber;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<InteractorProvider> CreateAsync(string databaseDirectoryPath, IServiceMediator axusServiceMediator, IBytesPool bytesPool, CancellationToken cancellationToken = default)
    {
        var result = new InteractorProvider(databaseDirectoryPath, axusServiceMediator, bytesPool);
        await result.InitAsync(cancellationToken);
        return result;
    }

    internal InteractorProvider(string databaseDirectoryPath, IServiceMediator axusServiceMediator, IBytesPool bytesPool)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        _axusServiceMediator = axusServiceMediator;
        _bytesPool = bytesPool;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        if (_axusServiceMediator is null) throw new NullReferenceException(nameof(_axusServiceMediator));

        _initialNodesFetcher = await InitialNodesFetcher.CreateAsync(_axusServiceMediator, cancellationToken);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_databaseDirectoryPath, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(_axusServiceMediator, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(_axusServiceMediator, SingleValueFileStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

        var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_databaseDirectoryPath, "profile_publisher"));
        _profilePublisher = await ProfilePublisher.CreateAsync(_axusServiceMediator, SingleValueFileStorage.Factory, _bytesPool, profilePublisherOptions, cancellationToken);

        var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_databaseDirectoryPath, "profile_subscriber"));
        _profileSubscriber = await ProfileSubscriber.CreateAsync(_axusServiceMediator, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions, cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_profilePublisher is not null) await _profilePublisher.DisposeAsync();
        _profilePublisher = null;

        if (_profileSubscriber is not null) await _profileSubscriber.DisposeAsync();
        _profileSubscriber = null;

        if (_fileUploader is not null) await _fileUploader.DisposeAsync();
        _fileUploader = null;

        if (_fileDownloader is not null) await _fileDownloader.DisposeAsync();
        _fileDownloader = null;

        if (_initialNodesFetcher is not null) await _initialNodesFetcher.DisposeAsync();
        _initialNodesFetcher = null;
    }

    public IFileDownloader GetFileDownloader() => _fileDownloader!;

    public IFileUploader GetFileUploader() => _fileUploader!;

    public IProfilePublisher GetProfilePublisher() => _profilePublisher!;

    public IProfileSubscriber GetProfileSubscriber() => _profileSubscriber!;
}
