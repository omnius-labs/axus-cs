using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public sealed class FileDownloader : AsyncDisposableBase, IFileDownloader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IKeyValueStorageFactory _keyValueStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly FileDownloaderOptions _options;

        private readonly ISingleValueStorage _configStorage;
        private readonly FileDownloaderRepository _fileDownloaderRepo;

        private Task _watchLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.FileDownloader";

        public static async ValueTask<FileDownloader> CreateAsync(IXeusService xeusService, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, FileDownloaderOptions options, CancellationToken cancellationToken = default)
        {
            var fileDownloader = new FileDownloader(xeusService, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
            await fileDownloader.InitAsync(cancellationToken);
            return fileDownloader;
        }

        private FileDownloader(IXeusService xeusService, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, FileDownloaderOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _keyValueStorageFactory = keyValueStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
            _fileDownloaderRepo = new FileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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
        }

        private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);

                for (; ; )
                {
                    await Task.Delay(1000 * 30, cancellationToken);

                    await this.SyncSubscribedFiles(cancellationToken);
                    await this.TryExportSubscribedFiles(cancellationToken);
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

        private async ValueTask SyncSubscribedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var subscribedFileReports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
                var hashes = new HashSet<OmniHash>();
                hashes.UnionWith(subscribedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash));

                foreach (var hash in hashes)
                {
                    if (_fileDownloaderRepo.Items.Exists(hash)) continue;
                    await _service.UnsubscribeFileAsync(hash, Registrant, cancellationToken);
                }

                foreach (var seed in _fileDownloaderRepo.Items.FindAll().Select(n => n.Seed))
                {
                    if (hashes.Contains(seed.RootHash)) continue;
                    await _service.SubscribeFileAsync(seed.RootHash, Registrant, cancellationToken);
                }
            }
        }

        private async ValueTask TryExportSubscribedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var config = await this.GetConfigAsync(cancellationToken);
                var basePath = config.DestinationDirectory;
                DirectoryHelper.CreateDirectory(basePath);

                foreach (var item in _fileDownloaderRepo.Items.FindAll())
                {
                    if (item.State == DownloadingFileState.Completed) continue;

                    var filePath = Path.Combine(basePath, item.Seed.Name);

                    if (await _service.TryExportFileToStorageAsync(item.Seed.RootHash, filePath, cancellationToken))
                    {
                        var newItem = new DownloadingFileItem(item.Seed, filePath, item.CreationTime, DownloadingFileState.Completed);
                        _fileDownloaderRepo.Items.Upsert(newItem);
                    }
                }
            }
        }

        public async ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var reports = new List<DownloadingFileReport>();

                foreach (var item in _fileDownloaderRepo.Items.FindAll())
                {
                    reports.Add(new DownloadingFileReport(item.Seed, item.FilePath, item.CreationTime, item.State));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(Seed seed, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var now = DateTime.UtcNow;
                var item = new DownloadingFileItem(seed, null, now, DownloadingFileState.Downloading);
                _fileDownloaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(Seed seed, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                _fileDownloaderRepo.Items.Delete(seed);
            }
        }

        public async ValueTask<FileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var config = await _configStorage.TryGetValueAsync<FileDownloaderConfig>(cancellationToken);
                if (config is null) return FileDownloaderConfig.Empty;

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
}
