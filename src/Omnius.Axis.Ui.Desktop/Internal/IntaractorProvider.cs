using Omnius.Axis.Intaractors;
using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface IIntaractorProvider : IAsyncDisposable
{
    IFileDownloader GetFileDownloader();

    IFileUploader GetFileUploader();

    IProfilePublisher GetProfilePublisher();

    IProfileSubscriber GetProfileSubscriber();
}

public partial class IntaractorProvider : AsyncDisposableBase, IIntaractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string _databaseDirectoryPath;
    private IAxisServiceMediator? _axisServiceMediator;
    private readonly IBytesPool _bytesPool;

    private IInitialNodesFetcher? _initialNodesFetcher;
    private IFileDownloader? _fileDownloader;
    private IFileUploader? _fileUploader;
    private IProfilePublisher? _profilePublisher;
    private IProfileSubscriber? _profileSubscriber;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<IntaractorProvider> CreateAsync(string databaseDirectoryPath, IAxisServiceMediator axisServiceMediator, IBytesPool bytesPool, CancellationToken cancellationToken = default)
    {
        var result = new IntaractorProvider(databaseDirectoryPath, axisServiceMediator, bytesPool);
        await result.InitAsync(cancellationToken);
        return result;
    }

    internal IntaractorProvider(string databaseDirectoryPath, IAxisServiceMediator axisServiceMediator, IBytesPool bytesPool)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        _axisServiceMediator = axisServiceMediator;
        _bytesPool = bytesPool;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        if (_axisServiceMediator is null) throw new NullReferenceException(nameof(_axisServiceMediator));

        _initialNodesFetcher = await InitialNodesFetcher.CreateAsync(_axisServiceMediator, cancellationToken);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_databaseDirectoryPath, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(_axisServiceMediator, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(_axisServiceMediator, SingleValueFileStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

        var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_databaseDirectoryPath, "profile_publisher"));
        _profilePublisher = await ProfilePublisher.CreateAsync(_axisServiceMediator, SingleValueFileStorage.Factory, _bytesPool, profilePublisherOptions, cancellationToken);

        var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_databaseDirectoryPath, "profile_subscriber"));
        _profileSubscriber = await ProfileSubscriber.CreateAsync(_axisServiceMediator, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions, cancellationToken);
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
