using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Interactors;

public sealed class FileUploader : AsyncDisposableBase, IFileUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly FileUploaderOptions _options;

    private readonly FileUploaderRepository _fileUploaderRepo;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Author = "file_uploader/v1";

    public static async ValueTask<FileUploader> CreateAsync(IServiceMediator serviceController, IBytesPool bytesPool, FileUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var fileUploader = new FileUploader(serviceController, bytesPool, options);
        await fileUploader.InitAsync(cancellationToken);
        return fileUploader;
    }

    private FileUploader(IServiceMediator service, IBytesPool bytesPool, FileUploaderOptions options)
    {
        _serviceController = service;
        _bytesPool = bytesPool;
        _options = options;

        _fileUploaderRepo = new FileUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _fileUploaderRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _fileUploaderRepo.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

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

    private async Task SyncPublishedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceController.GetPublishedFileReportsAsync(cancellationToken);
            var filePaths = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.FilePath)
                .Where(n => n is not null)
                .Select(n => n!.ToString())
                .ToHashSet();

            foreach (var filePath in filePaths)
            {
                if (_fileUploaderRepo.FileItems.Exists(filePath)) continue;
                await _serviceController.UnpublishFileFromStorageAsync(filePath, Author, cancellationToken);
            }

            foreach (var fileItem in _fileUploaderRepo.FileItems.FindAll())
            {
                if (filePaths.Contains(fileItem.FilePath)) continue;
                var rootHash = await _serviceController.PublishFileFromStorageAsync(fileItem.FilePath, 8 * 1024 * 1024, Author, cancellationToken);

                var seed = new Seed(rootHash, fileItem.Name, (ulong)fileItem.Length, Timestamp64.FromDateTime(fileItem.CreatedTime));
                var newFileItem = fileItem with
                {
                    Seed = seed,
                };
                _fileUploaderRepo.FileItems.Upsert(newFileItem);
            }
        }
    }

    public async ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<UploadingFileReport>();

            foreach (var item in _fileUploaderRepo.FileItems.FindAll())
            {
                var status = new UploadingFileStatus(item.State);
                reports.Add(new UploadingFileReport(item.FilePath, item.Seed, item.CreatedTime, status));
            }

            return reports;
        }
    }

    public async ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_fileUploaderRepo.FileItems.Exists(filePath)) return;

            var now = DateTime.UtcNow;
            var fileItem = new UploadingFileItem
            {
                FilePath = filePath,
                Name = name,
                Length = new FileInfo(filePath).Length,
                State = UploadingFileState.Waiting,
                CreatedTime = now,
            };
            _fileUploaderRepo.FileItems.Upsert(fileItem);
        }
    }

    public async ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (!_fileUploaderRepo.FileItems.Exists(filePath)) return;

            await _serviceController.UnpublishFileFromStorageAsync(filePath, Author, cancellationToken);

            _fileUploaderRepo.FileItems.Delete(filePath);
        }
    }
}
