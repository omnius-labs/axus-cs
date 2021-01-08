using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Extensions;
using Omnius.Core.Io;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class ContentPublisher : AsyncDisposableBase, IContentPublisher
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ContentPublisherOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly ContentPublisherRepository _repository;

        private const int MaxBlockLength = 32 * 1024 * 1024;

        internal sealed class ContentPublisherFactory : IContentPublisherFactory
        {
            public async ValueTask<IContentPublisher> CreateAsync(ContentPublisherOptions options, IBytesPool bytesPool)
            {
                var result = new ContentPublisher(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IContentPublisherFactory Factory { get; } = new ContentPublisherFactory();

        private ContentPublisher(ContentPublisherOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new ContentPublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "content-publisher.db"));
        }

        internal async ValueTask InitAsync()
        {
            await _repository.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _repository.Dispose();
        }

        public async ValueTask<ContentPublisherReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var pushContentReports = new List<ContentPublisherItemReport>();

            foreach (var item in _repository.Items.FindAll())
            {
                pushContentReports.Add(new ContentPublisherItemReport(item.FilePath, item.ContentHash, item.Registrant));
            }

            return new ContentPublisherReport(pushContentReports.ToArray());
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniHash>();

            foreach (var item in _repository.Items.FindAll())
            {
                results.Add(item.ContentHash);
            }

            return results;
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash contentHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            if (exists.HasValue && !exists.Value)
            {
                return Enumerable.Empty<OmniHash>();
            }

            var item = _repository.Items.Find(contentHash).FirstOrDefault();
            if (item is null)
            {
                return Enumerable.Empty<OmniHash>();
            }

            return item.MerkleTreeSections.SelectMany(n => n.Hashes);
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash contentHash)
        {
            if (!_repository.Items.Exists(contentHash))
            {
                return false;
            }

            return true;
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash contentHash, OmniHash blockHash)
        {
            var item = _repository.Items.Find(contentHash).FirstOrDefault();
            if (item is null)
            {
                return false;
            }

            return item.MerkleTreeSections.Any(n => n.Contains(contentHash));
        }

        public async ValueTask<OmniHash> PublishContentAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
        {
            // 既にエンコード済みの場合
            {
                var item = _repository.Items.FindOne(filePath, registrant);
                if (item is not null)
                {
                    return item.ContentHash;
                }
            }

            // エンコード処理
            {
                var fileMerkleTreeSection = await this.EncodeFileAsync(filePath, cancellationToken);
                var (contentHash, middleMerkleTreeSections) = await this.EncodeMerkleTreeSectionAsync(fileMerkleTreeSection, cancellationToken);

                var item = new ContentPublisherItem(contentHash, filePath, registrant, middleMerkleTreeSections.Append(fileMerkleTreeSection).ToArray());
                _repository.Items.Insert(item);

                return contentHash;
            }
        }

        public async ValueTask<OmniHash> PublishContentAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default)
        {
            // エンコード処理
            {
                var bytesMerkleTreeSection = await this.EncodeBytesAsync(sequence, cancellationToken);
                var (contentHash, middleMerkleTreeSections) = await this.EncodeMerkleTreeSectionAsync(bytesMerkleTreeSection, cancellationToken);

                var item = new ContentPublisherItem(contentHash, null, registrant, middleMerkleTreeSections.Append(bytesMerkleTreeSection).ToArray());
                _repository.Items.Insert(item);

                return contentHash;
            }
        }

        private async ValueTask<MerkleTreeSection> EncodeFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // ファイルからハッシュ値を算出する
            using var inStream = new FileStream(filePath, FileMode.Open);

            var hashList = new List<OmniHash>();

            using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
            {
                var remain = inStream.Length;

                while (remain > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var blockLength = (int)Math.Min(remain, MaxBlockLength);
                    remain -= blockLength;

                    var memory = memoryOwner.Memory.Slice(0, blockLength);
                    await inStream.ReadAsync(memory, cancellationToken);

                    var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                    hashList.Add(hash);
                }
            }

            return new MerkleTreeSection(0, MaxBlockLength, (ulong)inStream.Length, hashList.ToArray());
        }

        private async ValueTask<MerkleTreeSection> EncodeBytesAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            var hashList = new List<OmniHash>();
            var sequenceLength = sequence.Length;

            using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
            {
                while (sequence.Length > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var blockLength = (int)Math.Min(sequence.Length, MaxBlockLength);

                    var memory = memoryOwner.Memory.Slice(0, blockLength);
                    sequence.CopyTo(memory.Span);
                    sequence = sequence.Slice(blockLength);

                    var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                    hashList.Add(hash);
                }
            }

            return new MerkleTreeSection(0, MaxBlockLength, (ulong)sequenceLength, hashList.ToArray());
        }

        private async ValueTask<(OmniHash, IEnumerable<MerkleTreeSection>)> EncodeMerkleTreeSectionAsync(MerkleTreeSection lastMerkleTreeSection, CancellationToken cancellationToken = default)
        {
            var resultMerkleTreeSections = new Stack<MerkleTreeSection>();

            var tempDirPath = Path.Combine(_options.ConfigDirectoryPath, "cache", "_temp_");

            // ハッシュ値からMerkle treeを作成する
            for (; ; )
            {
                using var hub = new BytesHub(_bytesPool);

                lastMerkleTreeSection.Export(hub.Writer, _bytesPool);

                if (hub.Writer.WrittenBytes > MaxBlockLength)
                {
                    var hashList = new List<OmniHash>();

                    using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
                    {
                        var sequence = hub.Reader.GetSequence();
                        var remain = sequence.Length;

                        while (remain > 0)
                        {
                            var blockLength = (int)Math.Min(remain, MaxBlockLength);
                            remain -= blockLength;

                            var memory = memoryOwner.Memory.Slice(0, blockLength);
                            sequence.CopyTo(memory.Span);

                            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                            hashList.Add(hash);

                            await this.WriteBlockAsync(tempDirPath, hash, memory);

                            sequence = sequence.Slice(blockLength);
                        }
                    }

                    var newMerkleTreeSection = new MerkleTreeSection(lastMerkleTreeSection.Depth + 1, MaxBlockLength, (ulong)hub.Writer.WrittenBytes, hashList.ToArray());
                    lastMerkleTreeSection = newMerkleTreeSection;
                    resultMerkleTreeSections.Push(newMerkleTreeSection);
                }
                else
                {
                    using var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength);
                    var sequence = hub.Reader.GetSequence();

                    var memory = memoryOwner.Memory.Slice(0, (int)sequence.Length);
                    sequence.CopyTo(memory.Span);

                    var contentHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

                    await this.WriteBlockAsync(tempDirPath, contentHash, memory);

                    // 一時フォルダからキャッシュフォルダへ移動させる
                    var cacheDirPath = this.ComputeCacheDirectoryPath(contentHash);
                    Directory.Move(tempDirPath, cacheDirPath);

                    return (contentHash, resultMerkleTreeSections.ToArray());
                }
            }
        }

        private async ValueTask WriteBlockAsync(string basePath, OmniHash hash, ReadOnlyMemory<byte> memory)
        {
            var filePath = Path.Combine(basePath, StringConverter.HashToString(hash));
            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
            await fileStream.WriteAsync(memory);
        }

        public async ValueTask UnpublishContentAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
        {
            var item = _repository.Items.FindOne(filePath, registrant);
            if (item == null)
            {
                return;
            }

            _repository.Items.Delete(filePath, registrant);

            this.TryDeleteCache(item.ContentHash);
        }

        public async ValueTask UnpublishContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default)
        {
            _repository.Items.Delete(contentHash, registrant);

            this.TryDeleteCache(contentHash);
        }

        private bool TryDeleteCache(OmniHash contentHash)
        {
            if (_repository.Items.Exists(contentHash))
            {
                return false;
            }

            // キャッシュフォルダを削除
            var cacheDirPath = this.ComputeCacheDirectoryPath(contentHash);
            Directory.Delete(cacheDirPath);

            return true;
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            var item = _repository.Items.Find(contentHash).FirstOrDefault();
            if (item is null)
            {
                return null;
            }

            var middleMerkleTreeSections = new List<MerkleTreeSection>();

            if (item.FilePath is not null)
            {
                var fileMerkleTreeSections = item.MerkleTreeSections[^1];
                if (fileMerkleTreeSections.TryGetIndex(blockHash, out var index))
                {
                    if (!File.Exists(item.FilePath))
                    {
                        return null;
                    }

                    var position = fileMerkleTreeSections.BlockLength * index;
                    var blockSize = (int)Math.Min(fileMerkleTreeSections.BlockLength, (int)(fileMerkleTreeSections.Length - (ulong)position));

                    using var fileStream = new UnbufferedFileStream(item.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                    fileStream.Seek(position, SeekOrigin.Begin);
                    var memoryOwner = _bytesPool.Memory.Rent(blockSize).Shrink(blockSize);
                    await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

                    return memoryOwner;
                }

                middleMerkleTreeSections.AddRange(item.MerkleTreeSections.ToArray()[..^1]);
            }
            else
            {
                middleMerkleTreeSections.AddRange(item.MerkleTreeSections);
            }

            if (middleMerkleTreeSections.Any(n => n.Contains(blockHash)))
            {
                string filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

                return memoryOwner;
            }

            return null;
        }

        private string ComputeCacheDirectoryPath(OmniHash contentHash)
        {
            return Path.Combine(_options.ConfigDirectoryPath, "cache", StringConverter.HashToString(contentHash));
        }

        private string ComputeCacheFilePath(OmniHash contentHash, OmniHash blockHash)
        {
            return Path.Combine(this.ComputeCacheDirectoryPath(contentHash), StringConverter.HashToString(blockHash));
        }
    }
}
