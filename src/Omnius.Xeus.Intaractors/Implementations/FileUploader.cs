using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public record UploadedFileReport
    {
        public UploadedFileReport(DateTime creationTime, string filePath)
        {
            this.CreationTime = creationTime;
            this.FilePath = filePath;
        }

        public DateTime CreationTime { get; }

        public string FilePath { get; }
    }

    public sealed class FileUploader : AsyncDisposableBase, IFileUploader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly FileUploaderOptions _options;

        private readonly FileUploaderRepository _fileUploaderRepo;

        private readonly Task _watchLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.FileUploader";

        public FileUploader(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, FileUploaderOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _fileUploaderRepo = new FileUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _watchLoopTask;

            _cancellationTokenSource.Dispose();
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
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async Task SyncPublishedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedFileReports = await _service.GetPublishedFileReportsAsync(cancellationToken);
                var filePaths = new HashSet<string>();
                filePaths.UnionWith(publishedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.FilePath).Where(n => n is not null)!);

                foreach (var filePath in filePaths)
                {
                    if (_fileUploaderRepo.Items.Exists(filePath)) continue;
                    await _service.UnpublishFileFromStorageAsync(filePath, Registrant, cancellationToken);
                }

                foreach (var filePath in _fileUploaderRepo.Items.FindAll().Select(n => n.FilePath))
                {
                    if (filePaths.Contains(filePath)) continue;
                    _fileUploaderRepo.Items.Delete(filePath);
                }
            }
        }

        public async ValueTask<IEnumerable<UploadedFileReport>> GetUploadedFileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var reports = new List<UploadedFileReport>();

                foreach (var item in _fileUploaderRepo.Items.FindAll())
                {
                    reports.Add(new UploadedFileReport(item.CreationTime.ToDateTime(), item.FilePath));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await _service.PublishFileFromStorageAsync(filePath, Registrant, cancellationToken);

                var item = new UploadingFileItem(filePath, Timestamp.FromDateTime(DateTime.UtcNow));
                _fileUploaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await _service.UnpublishFileFromStorageAsync(filePath, Registrant, cancellationToken);

                _fileUploaderRepo.Items.Delete(filePath);
            }
        }
    }
}
