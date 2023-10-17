using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed class FileUploader : AsyncDisposableBase, IFileUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly FileUploaderOptions _options;

    private readonly FileUploaderRepository _fileUploaderRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Zone = "file-uploader-v1";

    public static async ValueTask<FileUploader> CreateAsync(IAxusServiceMediator serviceController, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, FileUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var fileUploader = new FileUploader(serviceController, singleValueStorageFactory, bytesPool, options);
        await fileUploader.InitAsync(cancellationToken);
        return fileUploader;
    }

    private FileUploader(IAxusServiceMediator service, ISingleValueStorageFactory singleValueStorageFactory, IBytesPool bytesPool, FileUploaderOptions options)
    {
        _serviceController = service;
        _bytesPool = bytesPool;
        _options = options;

        _fileUploaderRepo = new FileUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
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
            var reports = await _serviceController.GetPublishedFileReportsAsync(Zone, cancellationToken);
            var filePaths = reports.Select(n => n.FilePath).Where(n => n is not null).Select(n => n!.ToString()).ToHashSet();

            foreach (var filePath in filePaths)
            {
                if (_fileUploaderRepo.FileItems.Exists(filePath)) continue;
                await _serviceController.UnpublishFileFromStorageAsync(filePath, Zone, cancellationToken);
            }

            foreach (var fileItem in _fileUploaderRepo.FileItems.FindAll())
            {
                if (filePaths.Contains(fileItem.FilePath)) continue;
                var rootHash = await _serviceController.PublishFileFromStorageAsync(fileItem.FilePath, 8 * 1024 * 1024, Zone, cancellationToken);

                var seed = new Seed(rootHash, fileItem.Name, (ulong)fileItem.Length, Timestamp64.FromDateTime(fileItem.CreatedTime));
                var newFileItem = fileItem with
                {
                    Seed = seed,
                };
                _fileUploaderRepo.FileItems.Upsert(newFileItem);
            }
        }
    }

    public async ValueTask<IEnumerable<FileUploadingReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<FileUploadingReport>();

            foreach (var item in _fileUploaderRepo.FileItems.FindAll())
            {
                var status = new FileUploadingStatus(item.State);
                reports.Add(new FileUploadingReport(item.FilePath, item.Seed, item.CreatedTime, status));
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
            var fileItem = new FileUploadingItem
            {
                FilePath = filePath,
                Name = name,
                Length = new FileInfo(filePath).Length,
                State = FileUploadingState.Waiting,
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

            await _serviceController.UnpublishFileFromStorageAsync(filePath, Zone, cancellationToken);

            _fileUploaderRepo.FileItems.Delete(filePath);
        }
    }
}
