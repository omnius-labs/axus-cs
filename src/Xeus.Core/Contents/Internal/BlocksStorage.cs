using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Configuration;
using Omnix.Cryptography;
using Omnix.Serialization;
using Xeus.Core.Internal;
using Xeus.Messages.Reports;

namespace Xeus.Core.Contents.Internal
{
    internal sealed partial class BlocksStorage : DisposableBase, IEnumerable<OmniHash>
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Stream _fileStream;

        private BufferPool _bufferPool;
        private UsingSectorsManager _usingSectorsManager;
        private ProtectionStatusManager _protectionStatusManager;

        private Settings _settings;

        private ulong _size;
        private Dictionary<OmniHash, ClusterInfo> _clusterInfoMap;

        private EventQueue<OmniHash> _addedBlockEventQueue = new EventQueue<OmniHash>(new TimeSpan(0, 0, 3));
        private EventQueue<OmniHash> _removedBlockEventQueue = new EventQueue<OmniHash>(new TimeSpan(0, 0, 3));
        private EventQueue<ErrorReport> _errorReportEventQueue = new EventQueue<ErrorReport>(new TimeSpan(0, 0, 3));

        private readonly object _lockObject = new object();

        private volatile bool _disposed;

        public static readonly uint SectorSize = 1024 * 256; // 256 KB

        public BlocksStorage(string configPath, string blocksPath, BufferPool bufferPool)
        {
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

            _bufferPool = bufferPool;
            _usingSectorsManager = new UsingSectorsManager(_bufferPool);
            _protectionStatusManager = new ProtectionStatusManager();

            _settings = new Settings(configPath);
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

                    foreach (var hash in _protectionStatusManager.GetProtectedHashes())
                    {
                        if (_clusterInfoMap.TryGetValue(hash, out var clusterInfo))
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
                    return _clusterInfoMap.Count;
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
            sectors = null;
            lock (_lockObject)
            {
                if (_usingSectorsManager.FreeSectorCount >= (uint)count)
                {
                    sectors = _usingSectorsManager.TakeFreeSectors(count);
                    return true;
                }
                else
                {
                    var removePairs = _clusterInfoMap
                        .Where(n => !_protectionStatusManager.Contains(n.Key))
                        .ToList();

                    removePairs.Sort((x, y) =>
                    {
                        return x.Value.LastAccessTime.CompareTo(y.Value.LastAccessTime);
                    });

                    foreach (var hash in removePairs.Select(n => n.Key))
                    {
                        this.Remove(hash);

                        if (_usingSectorsManager.FreeSectorCount >= 1024 * 4) break;
                    }

                    if (_usingSectorsManager.FreeSectorCount < (uint)count)
                    {
                        return false;
                    }

                    sectors = _usingSectorsManager.TakeFreeSectors(count);
                    return true;
                }
            }
        }

        public void Lock(OmniHash hash)
        {
            lock (_lockObject)
            {
                _protectionStatusManager.Add(hash);
            }
        }

        public void Unlock(OmniHash hash)
        {
            lock (_lockObject)
            {
                _protectionStatusManager.Remove(hash);
            }
        }

        public bool Contains(OmniHash hash)
        {
            lock (_lockObject)
            {
                return _clusterInfoMap.ContainsKey(hash);
            }
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            lock (_lockObject)
            {
                foreach (var hash in collection)
                {
                    if (_clusterInfoMap.ContainsKey(hash))
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
                    if (!_clusterInfoMap.ContainsKey(hash))
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
                if (_clusterInfoMap.TryGetValue(hash, out var clusterInfo))
                {
                    _clusterInfoMap.Remove(hash);

                    _usingSectorsManager.SetFreeSectors(clusterInfo.Sectors);

                    // Event
                    _removedBlockEventQueue.Enqueue(hash);
                }
            }
        }

        public void Resize(ulong size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

            lock (_lockObject)
            {
                uint unit = 1024 * 1024 * 256; // 256MB
                size = MathHelper.Roundup(size, unit);

                foreach (var key in _clusterInfoMap.Keys.ToArray()
                    .Where(n => _clusterInfoMap[n].Sectors.Any(point => size < (point * SectorSize) + SectorSize))
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
                _usingSectorsManager.Reallocate(_size);

                foreach (var indexes in _clusterInfoMap.Values.Select(n => n.Sectors))
                {
                    _usingSectorsManager.SetUsingSectors(indexes);
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
                    using (var bitmapManager = new BitmapManager(_bufferPool))
                    {
                        bitmapManager.SetLength(this.Size / SectorSize);

                        var hashes = new List<OmniHash>();

                        foreach (var (hash, clusterInfo) in _clusterInfoMap)
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

                                memoryOwner.Dispose();
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

        private byte[] _sectorBuffer = new byte[SectorSize];

        public bool TryGet(OmniHash hash, out IMemoryOwner<byte> memoryOwner)
        {
            if (!EnumHelper.IsValid(hash.AlgorithmType)) throw new ArgumentException($"Incorrect HashAlgorithmType: {hash.AlgorithmType}");

            memoryOwner = null;
            bool success = false;

            try
            {
                lock (_lockObject)
                {
                    ClusterInfo clusterInfo = null;

                    if (_clusterInfoMap.TryGetValue(hash, out clusterInfo))
                    {
                        clusterInfo = new ClusterInfo(clusterInfo.Sectors.ToArray(), clusterInfo.Length, Timestamp.FromDateTime(DateTime.UtcNow));
                        _clusterInfoMap[hash] = clusterInfo;
                    }

                    if (clusterInfo == null) return false;

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
            if (!EnumHelper.IsValid(hash.AlgorithmType)) throw new ArgumentException($"Incorrect HashAlgorithmType: {hash.AlgorithmType}");

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

                ulong[] sectors;

                if (!this.TryGetFreeSectors((int)((value.Length + (SectorSize - 1)) / SectorSize), out sectors))
                {
                    _errorReportEventQueue.Enqueue(new ErrorReport(ErrorReportType.SpaceNotFound, Timestamp.FromDateTime(DateTime.UtcNow)));

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

                _clusterInfoMap[hash] = new ClusterInfo(sectors, (uint)value.Length, Timestamp.FromDateTime(DateTime.UtcNow));

                // Event
                _addedBlockEventQueue.Enqueue(hash);

                return true;
            }
        }

        public uint GetLength(OmniHash hash)
        {
            lock (_lockObject)
            {
                if (_clusterInfoMap.TryGetValue(hash, out var value))
                {
                    return value.Length;
                }

                return 0;
            }
        }

        #region ISettings

        public void Load()
        {
            lock (_lockObject)
            {
                var config = _settings.Load<BlocksStorageConfig>("Config");

                _size = config.Size;
                _clusterInfoMap = new Dictionary<OmniHash, ClusterInfo>(config.ClusterInfoMap);

                this.UpdateUsingSectors();
            }
        }

        public void Save()
        {
            lock (_lockObject)
            {
                var config = new BlocksStorageConfig(0, _clusterInfoMap, _size);
                _settings.Save("Config", config);
            }
        }

        #endregion

        public OmniHash[] ToArray()
        {
            lock (_lockObject)
            {
                return _clusterInfoMap.Keys.ToArray();
            }
        }

        public IEnumerator<OmniHash> GetEnumerator()
        {
            lock (_lockObject)
            {
                foreach (var hash in _clusterInfoMap.Keys)
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

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _addedBlockEventQueue.Dispose();
                _removedBlockEventQueue.Dispose();

                if (_usingSectorsManager != null)
                {
                    _usingSectorsManager.Dispose();
                    _usingSectorsManager = null;
                }

                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }
        }
    }
}
