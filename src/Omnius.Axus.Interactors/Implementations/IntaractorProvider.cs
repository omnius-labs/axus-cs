using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public partial class InteractorProvider : AsyncDisposableBase, IInteractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string _databaseDirectoryPath;
    private IAxusServiceMediator? _serviceMediator;
    private readonly IBytesPool _bytesPool;

    private IProfileUploader? _profileUploader;
    private IProfileDownloader? _profileDownloader;
    private IMemoUploader? _barkUploader;
    private IMemoDownloader? _barkDownloader;
    private IFileUploader? _fileUploader;
    private IFileDownloader? _fileDownloader;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<InteractorProvider> CreateAsync(string databaseDirectoryPath, IAxusServiceMediator serviceMediator, IBytesPool bytesPool, CancellationToken cancellationToken = default)
    {
        var result = new InteractorProvider(databaseDirectoryPath, serviceMediator, bytesPool);
        await result.InitAsync(cancellationToken);
        return result;
    }

    internal InteractorProvider(string databaseDirectoryPath, IAxusServiceMediator serviceMediator, IBytesPool bytesPool)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        if (_serviceMediator is null) throw new NullReferenceException(nameof(_serviceMediator));

        var profileUploaderOptions = new ProfileUploaderOptions(Path.Combine(_databaseDirectoryPath, "profile_uploader"));
        _profileUploader = await ProfileUploader.CreateAsync(_serviceMediator, SingleValueFileStorage.Factory, _bytesPool, profileUploaderOptions, cancellationToken);

        var profileDownloaderOptions = new ProfileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "profile_downloader"));
        _profileDownloader = await ProfileDownloader.CreateAsync(_serviceMediator, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileDownloaderOptions, cancellationToken);

        var barkUploaderOptions = new MemoUploaderOptions(Path.Combine(_databaseDirectoryPath, "bark_uploader"));
        _barkUploader = await MemoUploader.CreateAsync(_serviceMediator, SingleValueFileStorage.Factory, _bytesPool, barkUploaderOptions, cancellationToken);

        var barkDownloaderOptions = new MemoDownloaderOptions(Path.Combine(_databaseDirectoryPath, "bark_downloader"));
        _barkDownloader = await MemoDownloader.CreateAsync(_profileDownloader, _serviceMediator, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, barkDownloaderOptions, cancellationToken);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_databaseDirectoryPath, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(_serviceMediator, SingleValueFileStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_databaseDirectoryPath, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(_serviceMediator, SingleValueFileStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_fileDownloader is not null) await _fileDownloader.DisposeAsync();
        _fileDownloader = null;

        if (_fileUploader is not null) await _fileUploader.DisposeAsync();
        _fileUploader = null;

        if (_barkDownloader is not null) await _barkDownloader.DisposeAsync();
        _barkDownloader = null;

        if (_barkUploader is not null) await _barkUploader.DisposeAsync();
        _barkUploader = null;

        if (_profileDownloader is not null) await _profileDownloader.DisposeAsync();
        _profileDownloader = null;

        if (_profileUploader is not null) await _profileUploader.DisposeAsync();
        _profileUploader = null;
    }

    public IProfileUploader GetProfileUploader() => _profileUploader!;
    public IProfileDownloader GetProfileDownloader() => _profileDownloader!;
    public IMemoUploader GetMemoUploader() => _barkUploader!;
    public IMemoDownloader GetMemoDownloader() => _barkDownloader!;
    public ISeedUploader GetSeedUploader() => throw new NotImplementedException();
    public ISeedDownloader GetSeedDownloader() => throw new NotImplementedException();
    public IFileUploader GetFileUploader() => _fileUploader!;
    public IFileDownloader GetFileDownloader() => _fileDownloader!;
}
