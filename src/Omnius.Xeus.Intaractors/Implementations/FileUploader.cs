using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public sealed class FileUploader : AsyncDisposableBase, IFileUploader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly FileUploaderOptions _options;

        private readonly FileUploaderRepository _fileUploaderRepo;

        private Task _watchLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.FileUploader";

        public static async ValueTask<FileUploader> CreateAsync(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, FileUploaderOptions options, CancellationToken cancellationToken = default)
        {
            var fileUploader = new FileUploader(xeusService, bytesStorageFactory, bytesPool, options);
            await fileUploader.InitAsync(cancellationToken);
            return fileUploader;
        }

        private FileUploader(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, FileUploaderOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _fileUploaderRepo = new FileUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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

                foreach (var item in _fileUploaderRepo.Items.FindAll())
                {
                    if (filePaths.Contains(item.FilePath)) continue;
                    var rootHash = await _service.PublishFileFromStorageAsync(item.FilePath, Registrant, cancellationToken);

                    var seed = new Seed(rootHash, item.Seed.Name, item.Seed.Size, item.Seed.CreationTime);
                    var newItem = new UploadingFileItem(item.FilePath, seed, item.CreationTime, UploadingFileState.Completed);

                    _fileUploaderRepo.Items.Upsert(newItem);
                }
            }
        }

        public async ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var reports = new List<UploadingFileReport>();

                foreach (var item in _fileUploaderRepo.Items.FindAll())
                {
                    var seed = (item.State == UploadingFileState.Completed) ? item.Seed : null;
                    reports.Add(new UploadingFileReport(item.FilePath, seed, item.CreationTime.ToDateTime(), item.State));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var now = Timestamp.FromDateTime(DateTime.UtcNow);
                var seed = new Seed(OmniHash.Empty, name, (ulong)new FileInfo(filePath).Length, now);
                var item = new UploadingFileItem(filePath, seed, now, UploadingFileState.Waiting);
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
