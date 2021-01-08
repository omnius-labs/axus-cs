using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Io;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class ContentSubscriber : AsyncDisposableBase, IContentSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ContentSubscriberOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly ContentSubscriberRepository _repository;

        private readonly Dictionary<OmniHash, OwnedContentStatus> _ownedContentStatusMap = new();
        private readonly object _ownedContentStatusMapLockObject = new();

        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal sealed class ContentSubscriberFactory : IContentSubscriberFactory
        {
            public async ValueTask<IContentSubscriber> CreateAsync(ContentSubscriberOptions options, IBytesPool bytesPool)
            {
                var result = new ContentSubscriber(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IContentSubscriberFactory Factory { get; } = new ContentSubscriberFactory();

        private ContentSubscriber(ContentSubscriberOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new ContentSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "want-content-storage.db"));
        }

        internal async ValueTask InitAsync()
        {
            await _repository.MigrateAsync();

            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _computeLoopTask;
            _cancellationTokenSource.Dispose();

            _repository.Dispose();
        }

        private async Task ComputeLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    await this.UpdateOwnedContentStatusMapAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw;
            }
        }

        private async ValueTask UpdateOwnedContentStatusMapAsync(CancellationToken cancellationToken = default)
        {
            var ownedContentStatuses = new List<OwnedContentStatus>();
            lock (_ownedContentStatusMapLockObject)
            {
                ownedContentStatuses.AddRange(_ownedContentStatusMap.Values);
            }

            foreach (var ownedContentStatus in ownedContentStatuses)
            {
                var lastMerkleTreeSection = ownedContentStatus.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth == 0)
                {
                    continue;
                }

                var completed = await this.ValidateDownloadCompletedAsync(ownedContentStatus.ContentHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (!completed)
                {
                    continue;
                }

                var newMerkleTreeSection = await this.DecodeMerkleTreeSectionAsync(ownedContentStatus.ContentHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (newMerkleTreeSection is null)
                {
                    continue;
                }

                lock (_ownedContentStatusMapLockObject)
                {
                    ownedContentStatus.MerkleTreeSections.Add(newMerkleTreeSection);
                }
            }
        }

        private async ValueTask<bool> ValidateDownloadCompletedAsync(OmniHash contentHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            foreach (var blockHash in blockHashes)
            {
                var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                if (!File.Exists(filePath))
                {
                    return false;
                }
            }

            return true;
        }

        private async ValueTask<MerkleTreeSection?> DecodeMerkleTreeSectionAsync(OmniHash contentHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            var hub = new BytesHub();

            foreach (var blockHash in blockHashes)
            {
                var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                int length = (int)fileStream.Length;
                await fileStream.ReadAsync(hub.Writer.GetMemory(length), cancellationToken);
                hub.Writer.Advance(length);
            }

            return MerkleTreeSection.Import(hub.Reader.GetSequence(), _bytesPool);
        }

        public async ValueTask<ContentSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var wantContentReports = new List<ContentSubscriberItemReport>();

            foreach (var item in _repository.Items.FindAll())
            {
                wantContentReports.Add(new ContentSubscriberItemReport(item.ContentHash, item.Registrant));
            }

            return new ContentSubscriberReport(wantContentReports.ToArray());
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniHash>();

            foreach (var status in _repository.Items.FindAll())
            {
                results.Add(status.ContentHash);
            }

            return results;
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash contentHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            if (exists.HasValue && !exists.Value)
            {
                return Enumerable.Empty<OmniHash>();
            }

            var items = _repository.Items.Find(contentHash);
            if (!items.Any())
            {
                return Enumerable.Empty<OmniHash>();
            }

            lock (_ownedContentStatusMapLockObject)
            {
                if (!_ownedContentStatusMap.TryGetValue(contentHash, out var ownedContentStatus))
                {
                    return Enumerable.Empty<OmniHash>();
                }

                var blockHashes = ownedContentStatus.MerkleTreeSections.SelectMany(n => n.Hashes).ToArray();

                if (exists is null)
                {
                    return blockHashes;
                }
                else
                {
                    var filteredHashes = new List<OmniHash>();

                    foreach (var blockHash in blockHashes)
                    {
                        var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                        if (File.Exists(filePath) != exists.Value)
                        {
                            continue;
                        }

                        filteredHashes.Add(blockHash);
                    }

                    return filteredHashes;
                }
            }
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash contentHash)
        {
            var items = _repository.Items.Find(contentHash);
            if (!items.Any())
            {
                return false;
            }

            return true;
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash contentHash, OmniHash blockHash)
        {
            var items = _repository.Items.Find(contentHash);
            if (!items.Any())
            {
                return false;
            }

            var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
            if (!File.Exists(filePath))
            {
                return false;
            }

            return true;
        }

        public async ValueTask SubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default)
        {
            _repository.Items.Insert(new ContentSubscriberItem(contentHash, registrant));
        }

        public async ValueTask UnsubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default)
        {
            var item = _repository.Items.FindOne(contentHash, registrant);
            if (item == null)
            {
                return;
            }

            _repository.Items.Delete(contentHash);

            // キャッシュフォルダを削除
            var cacheDirPath = this.ComputeCacheDirectoryPath(contentHash);
            Directory.Delete(cacheDirPath);

            // 所有しているコンテンツの情報を削除
            lock (_ownedContentStatusMapLockObject)
            {
                _ownedContentStatusMap.Remove(contentHash);
            }
        }

        public async ValueTask ExportContentAsync(OmniHash contentHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            var items = _repository.Items.Find(contentHash);
            if (!items.Any())
            {
                return;
            }

            OwnedContentStatus? ownedContentStatus;

            lock (_ownedContentStatusMapLockObject)
            {
                if (!_ownedContentStatusMap.TryGetValue(contentHash, out ownedContentStatus))
                {
                    return;
                }
            }

            var lastMerkleTreeSection = ownedContentStatus.MerkleTreeSections[^1];
            if (lastMerkleTreeSection.Depth != 0)
            {
                return;
            }

            foreach (var blockHash in lastMerkleTreeSection.Hashes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                if (!File.Exists(filePath))
                {
                    return;
                }

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                int length = (int)fileStream.Length;
                await fileStream.ReadAsync(bufferWriter.GetMemory(length), cancellationToken);
                bufferWriter.Advance(length);
            }
        }

        public async ValueTask ExportContentAsync(OmniHash contentHash, string filePath, CancellationToken cancellationToken = default)
        {
            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
            var writer = PipeWriter.Create(fileStream);
            await this.ExportContentAsync(contentHash, writer, cancellationToken);
            await writer.CompleteAsync();
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            var status = _repository.Items.Get(contentHash);
            if (status == null)
            {
                return null;
            }

            var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
            if (!File.Exists(filePath))
            {
                return null;
            }

            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
            var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
            await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

            return memoryOwner;
        }

        public async ValueTask WriteBlockAsync(OmniHash contentHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            var status = _repository.Items.Get(contentHash);
            if (status == null)
            {
                return;
            }

            lock (_ownedContentStatusMapLockObject)
            {
                if (!_ownedContentStatusMap.TryGetValue(contentHash, out var ownedContentStatus))
                {
                    return;
                }

                if (!ownedContentStatus.MerkleTreeSections.Any(n => n.Contains(blockHash)))
                {
                    return;
                }
            }

            var filePath = this.ComputeCacheFilePath(contentHash, blockHash);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (directoryPath is not null)
            {
                DirectoryHelper.CreateDirectory(directoryPath);
            }

            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
            await fileStream.WriteAsync(memory, cancellationToken);
        }

        private string ComputeCacheDirectoryPath(OmniHash contentHash)
        {
            return Path.Combine(_options.ConfigDirectoryPath, "cache", StringConverter.HashToString(contentHash));
        }

        private string ComputeCacheFilePath(OmniHash contentHash, OmniHash blockHash)
        {
            return Path.Combine(this.ComputeCacheDirectoryPath(contentHash), StringConverter.HashToString(blockHash));
        }

        private sealed class OwnedContentStatus
        {
            public OwnedContentStatus(OmniHash contentHash)
            {
                this.ContentHash = contentHash;
            }

            public OmniHash ContentHash { get; }

            public List<MerkleTreeSection> MerkleTreeSections { get; } = new List<MerkleTreeSection>();
        }
    }
}
