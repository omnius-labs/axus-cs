using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Intaractors.Internal.Repositories;
using Omnius.Axis.Intaractors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axis.Intaractors;

public sealed class FileUploader : AsyncDisposableBase, IFileUploader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxisServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly FileUploaderOptions _options;

    private readonly FileUploaderRepository _fileUploaderRepo;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Registrant = "Omnius.Axis.Intaractors.FileUploader";

    public static async ValueTask<FileUploader> CreateAsync(IAxisServiceMediator serviceController, IBytesPool bytesPool, FileUploaderOptions options, CancellationToken cancellationToken = default)
    {
        var fileUploader = new FileUploader(serviceController, bytesPool, options);
        await fileUploader.InitAsync(cancellationToken);
        return fileUploader;
    }

    private FileUploader(IAxisServiceMediator service, IBytesPool bytesPool, FileUploaderOptions options)
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
                await Task.Delay(1000 * 30, cancellationToken);

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
            var publishedFileReports = await _serviceController.GetPublishedFileReportsAsync(cancellationToken);
            var filePaths = new HashSet<string>();
            filePaths.UnionWith(publishedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.FilePath).Where(n => n is not null)!);

            foreach (var filePath in filePaths)
            {
                if (_fileUploaderRepo.Items.Exists(filePath)) continue;
                await _serviceController.UnpublishFileFromStorageAsync(filePath, Registrant, cancellationToken);
            }

            foreach (var item in _fileUploaderRepo.Items.FindAll())
            {
                if (filePaths.Contains(item.FilePath)) continue;
                var rootHash = await _serviceController.PublishFileFromStorageAsync(item.FilePath, Registrant, cancellationToken);

                var seed = new Seed(rootHash, item.Seed.Name, item.Seed.Size, item.Seed.CreatedTime);
                var newItem = new UploadingFileItem(item.FilePath, seed, item.CreatedTime, UploadingFileState.Completed);

                _fileUploaderRepo.Items.Upsert(newItem);
            }
        }
    }

    public async ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<UploadingFileReport>();

            foreach (var item in _fileUploaderRepo.Items.FindAll())
            {
                var seed = (item.State == UploadingFileState.Completed) ? item.Seed : null;
                reports.Add(new UploadingFileReport(item.FilePath, seed, item.CreatedTime, item.State));
            }

            return reports;
        }
    }

    public async ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_fileUploaderRepo.Items.Exists(filePath)) return;

            var now = DateTime.UtcNow;
            var seed = new Seed(OmniHash.Empty, name, (ulong)new FileInfo(filePath).Length, Timestamp.FromDateTime(now));
            var item = new UploadingFileItem(filePath, seed, now, UploadingFileState.Waiting);
            _fileUploaderRepo.Items.Upsert(item);
        }
    }

    public async ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (!_fileUploaderRepo.Items.Exists(filePath)) return;

            await _serviceController.UnpublishFileFromStorageAsync(filePath, Registrant, cancellationToken);

            _fileUploaderRepo.Items.Delete(filePath);
        }
    }
}
