using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class FileDownloader : AsyncDisposableBase, IFileDownloader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _service;
    private readonly IBytesPool _bytesPool;
    private readonly FileDownloaderOptions _options;

    private readonly FileDownloaderRepository _fileDownloaderRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Author = "file_downloader/v1";

    public static async ValueTask<FileDownloader> CreateAsync(IServiceMediator service, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, FileDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var fileDownloader = new FileDownloader(service, singleValueStorageFactory, bytesPool, options);
        await fileDownloader.InitAsync(cancellationToken);
        return fileDownloader;
    }

    private FileDownloader(IServiceMediator service, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, FileDownloaderOptions options)
    {
        _service = service;
        _bytesPool = bytesPool;
        _options = options;

        _fileDownloaderRepo = new FileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _fileDownloaderRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _fileDownloaderRepo.Dispose();
        _configStorage.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                await this.SyncSubscribedFiles(cancellationToken);
                await this.TryExportSubscribedFiles(cancellationToken);
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

    private async ValueTask SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
            var hashes = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.RootHash)
                .ToHashSet();

            foreach (var hash in hashes)
            {
                if (_fileDownloaderRepo.FileItems.Exists(hash)) continue;
                await _service.UnsubscribeFileAsync(hash, Author, cancellationToken);
            }

            foreach (var seed in _fileDownloaderRepo.FileItems.FindAll().Select(n => n.FileSeed))
            {
                if (hashes.Contains(seed.RootHash)) continue;
                await _service.SubscribeFileAsync(seed.RootHash, Author, cancellationToken);
            }
        }
    }

    private async ValueTask TryExportSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.GetConfigAsync(cancellationToken);
            var basePath = Path.GetFullPath(config.DestinationDirectory);

            var reports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
            var reportMap = reports
                .Where(n => n.Authors.Contains(Author))
                .ToDictionary(n => n.RootHash);

            foreach (var fileItem in _fileDownloaderRepo.FileItems.FindAll())
            {
                if (fileItem.State == DownloadingFileState.Completed) continue;

                if (!reportMap.TryGetValue(fileItem.FileSeed.RootHash, out var report)) continue;
                if (report.Status.State != SubscribedFileState.Downloaded) continue;

                DirectoryHelper.CreateDirectory(basePath);
                var filePath = Path.Combine(basePath, fileItem.FileSeed.Name);

                if (await _service.TryExportFileToStorageAsync(fileItem.FileSeed.RootHash, filePath, cancellationToken))
                {
                    var newFileItem = fileItem with
                    {
                        FilePath = filePath,
                        State = DownloadingFileState.Completed,
                    };
                    _fileDownloaderRepo.FileItems.Upsert(newFileItem);
                }
            }
        }
    }

    public async ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<DownloadingFileReport>();

            var reports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
            var reportMap = reports
                .Where(n => n.Authors.Contains(Author))
                .ToDictionary(n => n.RootHash);

            foreach (var item in _fileDownloaderRepo.FileItems.FindAll())
            {
                if (!reportMap.TryGetValue(item.FileSeed.RootHash, out var report)) continue;

                var status = new DownloadingFileStatus(report.Status.CurrentDepth, report.Status.DownloadedBlockCount,
                    report.Status.TotalBlockCount, item.State);
                results.Add(new DownloadingFileReport(item.FileSeed, item.FilePath, item.CreatedTime, status));
            }

            return results;
        }
    }

    public async ValueTask RegisterAsync(FileSeed fileSeed, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_fileDownloaderRepo.FileItems.Exists(fileSeed)) return;

            var now = DateTime.UtcNow;
            var fileItem = new DownloadingFileItem()
            {
                FileSeed = fileSeed,
                State = DownloadingFileState.Downloading,
                CreatedTime = now,
            };
            _fileDownloaderRepo.FileItems.Upsert(fileItem);
        }
    }

    public async ValueTask UnregisterAsync(FileSeed fileSeed, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (!_fileDownloaderRepo.FileItems.Exists(fileSeed)) return;

            _fileDownloaderRepo.FileItems.Delete(fileSeed);
        }
    }

    public async ValueTask<FileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<FileDownloaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new FileDownloaderConfig(
                    destinationDirectory: "../downloads"
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(FileDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
