using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Configuration;
using Omnix.Serialization.RocketPack;
using Xeus.Core.Internal.Helpers;
using Xeus.Core.Internal.Common;
using Xeus.Messages;

namespace Xeus.Core.Internal.Storage.Primitives
{
    internal sealed partial class BlockStorage : DisposableBase, ISettings, IEnumerable<OmniHash>
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Stream _fileStream;

        private readonly BufferPool _bufferPool;
        private readonly UsingSectorPool _usingSectorPool;
        private readonly ProtectionStatus _protectionStatus;

        private readonly SettingsDatabase _settings;

        private ulong _size;
        private readonly Dictionary<OmniHash, ClusterMetadata> _clusterMetadataMap = new Dictionary<OmniHash, ClusterMetadata>();

        private readonly LazyEvent<OmniHash> _addedBlockEventQueue = new LazyEvent<OmniHash>(new TimeSpan(0, 0, 3));
        private readonly LazyEvent<OmniHash> _removedBlockEventQueue = new LazyEvent<OmniHash>(new TimeSpan(0, 0, 3));
        private readonly LazyEvent<ErrorReport> _errorReportEventQueue = new LazyEvent<ErrorReport>(new TimeSpan(0, 0, 3));

        private readonly object _lockObject = new object();

        private volatile bool _disposed;

        public static readonly uint SectorSize = 1024 * 256; // 256 KB

        public BlockStorage(string basePath, BufferPool bufferPool)
        {
            var settingsPath = Path.Combine(basePath, "Settings");
            var childrenPath = Path.Combine(basePath, "Children");

            _bufferPool = bufferPool;

            _settings = new SettingsDatabase(settingsPath);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string blocksPath = Path.Combine(basePath, "Storage.blocks");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
                _fileStream = new FileStream(blocksPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, (int)SectorSize, FileFlagNoBuffering);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _fileStream = new FileStream(blocksPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, (int)SectorSize);
            }
            else
            {
                throw new NotSupportedException();
            }

            _usingSectorPool = new UsingSectorPool(_bufferPool);
            _protectionStatus = new ProtectionStatus();
        }

        public ulong UsingAreaSize
        {
            get
            {
                lock (_lockObject)
                {
                    return (ulong)_fileStream.Length;
                }
            }
        }

        public ulong ProtectionAreaSize
        {
            get
            {
                lock (_lockObject)
                {
                    ulong protectionAreaSize = 0;

                    foreach (var hash in _protectionStatus.GetProtectedHashes())
                    {
                        if (_clusterMetadataMap.TryGetValue(hash, out var clusterInfo))
                        {
                            protectionAreaSize += (ulong)(SectorSize * clusterInfo.Sectors.Count);
                        }
                    }

                    return protectionAreaSize;
                }
            }
        }

        public ulong Size
        {
            get
            {
                lock (_lockObject)
                {
                    return _size;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _clusterMetadataMap.Count;
                }
            }
        }

        public event Action<IEnumerable<OmniHash>> AddedBlockEvents
        {
            add
            {
                _addedBlockEventQueue.Events += value;
            }
            remove
            {
                _addedBlockEventQueue.Events -= value;
            }
        }

        public event Action<IEnumerable<OmniHash>> RemovedBlockEvents
        {
            add
            {
                _removedBlockEventQueue.Events += value;
            }
            remove
            {
                _removedBlockEventQueue.Events -= value;
            }
        }

        public event Action<IEnumerable<ErrorReport>> ErrorReportEvents
        {
            add
            {
                _errorReportEventQueue.Events += value;
            }
            remove
            {
                _errorReportEventQueue.Events -= value;
            }
        }

        private bool TryGetFreeSectors(int count, out ulong[] sectors)
        {
            sectors = Array.Empty<ulong>();

            lock (_lockObject)
            {
                if (_usingSectorPool.FreeSectorCount >= (uint)count)
                {
                    sectors = _usingSectorPool.TakeFreeSectors(count);
                    return true;
                }
                else
                {
                    var removePairs = _clusterMetadataMap
                        .Where(n => !_protectionStatus.Contains(n.Key))
                        .ToList();

                    removePairs.Sort((x, y) =>
                    {
                        return x.Value.LastAccessTime.CompareTo(y.Value.LastAccessTime);
                    });

                    foreach (var hash in removePairs.Select(n => n.Key))
                    {
                        this.Remove(hash);

                        if (_usingSectorPool.FreeSectorCount >= 1024 * 4)
                        {
                            break;
                        }
                    }

                    if (_usingSectorPool.FreeSectorCount < (uint)count)
                    {
                        return false;
                    }

                    sectors = _usingSectorPool.TakeFreeSectors(count);
                    return true;
                }
            }
        }

        public void Lock(OmniHash hash)
        {
            lock (_lockObject)
            {
                _protectionStatus.Add(hash);
            }
        }

        public void Unlock(OmniHash hash)
        {
            lock (_lockObject)
            {
                _protectionStatus.Remove(hash);
            }
        }

        public bool Contains(OmniHash hash)
        {
            lock (_lockObject)
            {
                return _clusterMetadataMap.ContainsKey(hash);
            }
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            lock (_lockObject)
            {
                foreach (var hash in collection)
                {
                    if (_clusterMetadataMap.ContainsKey(hash))
                    {
                        yield return hash;
                    }
                }
            }
        }

        public IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection)
        {
            lock (_lockObject)
            {
                foreach (var hash in collection)
                {
                    if (!_clusterMetadataMap.ContainsKey(hash))
                    {
                        yield return hash;
                    }
                }
            }
        }

        public void Remove(OmniHash hash)
        {
            lock (_lockObject)
            {
                if (_clusterMetadataMap.TryGetValue(hash, out var clusterInfo))
                {
                    _clusterMetadataMap.Remove(hash);

                    _usingSectorPool.SetFreeSectors(clusterInfo.Sectors);

                    // Event
                    _removedBlockEventQueue.Enqueue(hash);
                }
            }
        }

        public void Resize(ulong size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            lock (_lockObject)
            {
                uint unit = 1024 * 1024 * 256; // 256MB
                size = MathHelper.Roundup(size, unit);

                foreach (var key in _clusterMetadataMap.Keys.ToArray()
                    .Where(n => _clusterMetadataMap[n].Sectors.Any(point => size < (point * SectorSize) + SectorSize))
                    .ToArray())
                {
                    this.Remove(key);
                }

                _size = MathHelper.Roundup(size, SectorSize);
                _fileStream.SetLength((long)Math.Min(_size, (ulong)_fileStream.Length));

                this.UpdateUsingSectors();
            }
        }

        private void UpdateUsingSectors()
        {
            lock (_lockObject)
            {
                _usingSectorPool.Reallocate(_size);

                foreach (var indexes in _clusterMetadataMap.Values.Select(n => n.Sectors))
                {
                    _usingSectorPool.SetUsingSectors(indexes);
                }
            }
        }

        public async ValueTask CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            await Task.Run(() =>
            {
                // 重複するセクタを確保したブロックを検出しRemoveする。
                lock (_lockObject)
                {
                    using (var bitmapManager = new BitStorage(_bufferPool))
                    {
                        bitmapManager.SetLength(this.Size / SectorSize);

                        var hashes = new List<OmniHash>();

                        foreach (var (hash, clusterInfo) in _clusterMetadataMap)
                        {
                            foreach (var sector in clusterInfo.Sectors)
                            {
                                if (!bitmapManager.Get(sector))
                                {
                                    bitmapManager.Set(sector, true);
                                }
                                else
                                {
                                    hashes.Add(hash);

                                    break;
                                }
                            }
                        }

                        foreach (var hash in hashes)
                        {
                            this.Remove(hash);
                        }
                    }
                }

                // 読めないブロックを検出しRemoveする。
                {
                    var list = this.ToArray();

                    uint badCount = 0;
                    uint checkedCount = 0;
                    uint blockCount = (uint)list.Length;

                    token.ThrowIfCancellationRequested();

                    progress.Invoke(new CheckBlocksProgressReport(badCount, checkedCount, blockCount));

                    foreach (var hash in list)
                    {
                        token.ThrowIfCancellationRequested();

                        lock (_lockObject)
                        {
                            if (this.Contains(hash))
                            {
                                bool result = this.TryGet(hash, out var memoryOwner);

                                if (!result)
                                {
                                    badCount++;
                                }

                                memoryOwner?.Dispose();
                            }
                        }

                        checkedCount++;

                        if (checkedCount % 32 == 0)
                        {
                            progress.Invoke(new CheckBlocksProgressReport(badCount, checkedCount, blockCount));
                        }
                    }

                    progress.Invoke(new CheckBlocksProgressReport(badCount, checkedCount, blockCount));
                }
            }, token);
        }

        private readonly byte[] _sectorBuffer = new byte[SectorSize];

        public bool TryGet(OmniHash hash, out IMemoryOwner<byte>? memoryOwner)
        {
            if (!EnumHelper.IsValid(hash.AlgorithmType))
            {
                throw new ArgumentException($"Incorrect HashAlgorithmType: {hash.AlgorithmType}");
            }

            memoryOwner = null;
            bool success = false;

            try
            {
                lock (_lockObject)
                {

                    if (_clusterMetadataMap.TryGetValue(hash, out var clusterInfo))
                    {
                        clusterInfo = new ClusterMetadata(clusterInfo.Sectors.ToArray(), clusterInfo.Length, Timestamp.FromDateTime(DateTime.UtcNow));
                        _clusterMetadataMap[hash] = clusterInfo;
                    }

                    if (clusterInfo == null)
                    {
                        return false;
                    }

                    memoryOwner = _bufferPool.Rent((int)clusterInfo.Length);

                    try
                    {
                        uint remain = clusterInfo.Length;

                        for (int i = 0; i < clusterInfo.Sectors.Count; i++, remain -= SectorSize)
                        {
                            ulong position = clusterInfo.Sectors[i] * SectorSize;

                            if (position > (ulong)_fileStream.Length)
                            {
                                _logger.Debug($"position too large: {position}");

                                return false;
                            }

                            if ((ulong)_fileStream.Position != position)
                            {
                                _fileStream.Seek((long)position, SeekOrigin.Begin);
                            }

                            uint length = Math.Min(remain, SectorSize);

                            _fileStream.Read(_sectorBuffer, 0, _sectorBuffer.Length);
                            BytesOperations.Copy(_sectorBuffer, memoryOwner.Memory.Span.Slice((int)(SectorSize * i)), (int)length);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        return false;
                    }
                }

                if (hash.AlgorithmType == OmniHashAlgorithmType.Sha2_256
                    && BytesOperations.SequenceEqual(Sha2_256.ComputeHash(memoryOwner.Memory.Span), hash.Value.Span))
                {
                    success = true;

                    return true;
                }
                else
                {
                    _logger.Debug("Broken block.");

                    return false;
                }
            }
            finally
            {
                if (!success)
                {
                    if (memoryOwner != null)
                    {
                        memoryOwner.Dispose();
                        memoryOwner = null;
                    }

                    this.Remove(hash);
                }
            }
        }

        public bool TrySet(OmniHash hash, ReadOnlySpan<byte> value)
        {
            if (!EnumHelper.IsValid(hash.AlgorithmType))
            {
                throw new ArgumentException($"Incorrect HashAlgorithmType: {hash.AlgorithmType}");
            }

            if (value.Length > 1024 * 1024 * 32)
            {
                _logger.Debug($"{nameof(value)} too large.");

                return false;
            }

            if (hash.AlgorithmType == OmniHashAlgorithmType.Sha2_256
                && !BytesOperations.SequenceEqual(Sha2_256.ComputeHash(value), hash.Value.Span))
            {
                _logger.Debug("Broken block.");

                return false;
            }

            lock (_lockObject)
            {
                if (this.Contains(hash))
                {
                    _logger.Debug($"Already exist.");

                    return true;
                }


                if (!this.TryGetFreeSectors((int)((value.Length + (SectorSize - 1)) / SectorSize), out var sectors))
                {
                    _errorReportEventQueue.Enqueue(new ErrorReport(Timestamp.FromDateTime(DateTime.UtcNow), ErrorReportType.SpaceNotFound));

                    _logger.Debug("Space not found.");

                    return false;
                }

                try
                {
                    uint remain = (uint)value.Length;

                    for (int i = 0; i < sectors.Length && 0 < remain; i++, remain -= SectorSize)
                    {
                        ulong position = sectors[i] * SectorSize;

                        if ((ulong)_fileStream.Length < position + SectorSize)
                        {
                            const uint unit = 1024 * 1024 * 256; // 256MB
                            ulong size = MathHelper.Roundup((position + SectorSize), unit);

                            _fileStream.SetLength((long)Math.Min(size, this.Size));
                        }

                        if ((ulong)_fileStream.Position != position)
                        {
                            _fileStream.Seek((long)position, SeekOrigin.Begin);
                        }

                        uint length = Math.Min(remain, SectorSize);

                        BytesOperations.Copy(value.Slice((int)(SectorSize * i)), _sectorBuffer, (int)length);
                        BytesOperations.Zero(_sectorBuffer.AsSpan((int)length, (int)(_sectorBuffer.Length - length)));

                        _fileStream.Write(_sectorBuffer, 0, _sectorBuffer.Length);
                    }

                    _fileStream.Flush();
                }
                catch (Exception e)
                {
                    _logger.Debug(e);

                    return false;
                }

                _clusterMetadataMap[hash] = new ClusterMetadata(sectors, (uint)value.Length, Timestamp.FromDateTime(DateTime.UtcNow));

                // Event
                _addedBlockEventQueue.Enqueue(hash);

                return true;
            }
        }

        public uint GetLength(OmniHash hash)
        {
            lock (_lockObject)
            {
                if (_clusterMetadataMap.TryGetValue(hash, out var value))
                {
                    return value.Length;
                }

                return 0;
            }
        }

        #region ISettings

        private bool _loaded = false;
        private readonly AsyncLock _settingsAsyncLock = new AsyncLock();

        public async ValueTask LoadAsync()
        {
            using (await _settingsAsyncLock.LockAsync())
            {
                if (_loaded)
                {
                    throw new SettingsAlreadyLoadedException();
                }

                _loaded = true;

                _size = 0;
                _clusterMetadataMap.Clear();

                if (_settings.TryGetContent<BlockStorageConfig>("config", out var config))
                {
                    _size = config.Size;

                    foreach (var (key, value) in config.ClusterMetadataMap)
                    {
                        _clusterMetadataMap.Add(key, value);
                    }
                }

                this.UpdateUsingSectors();
            }
        }

        public async ValueTask SaveAsync()
        {
            using (await _settingsAsyncLock.LockAsync())
            {
                var config = new BlockStorageConfig(0, _size, _clusterMetadataMap);
                _settings.SetContent("config", config);
            }
        }

        #endregion

        public OmniHash[] ToArray()
        {
            lock (_lockObject)
            {
                return _clusterMetadataMap.Keys.ToArray();
            }
        }

        public IEnumerator<OmniHash> GetEnumerator()
        {
            lock (_lockObject)
            {
                foreach (var hash in _clusterMetadataMap.Keys)
                {
                    yield return hash;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lockObject)
            {
                return this.GetEnumerator();
            }
        }

        protected override void OnDispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                _settings.Dispose();

                _addedBlockEventQueue.Dispose();
                _removedBlockEventQueue.Dispose();

                _usingSectorPool.Dispose();
                _fileStream.Dispose();
            }
        }
    }
}
