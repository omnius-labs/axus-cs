using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Collections;
using Omnius.Configuration;
using Omnius.Correction;
using Omnius.Io;
using Omnius.Messaging;
using Omnius.Security;
using Omnius.Utils;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public sealed partial class BlocksManager : DisposableBase, ISettings, ISetOperators<Hash>, IEnumerable<Hash>
        {
            private Stream _fileStream;

            private BufferManager _bufferManager;
            private SectorsManager _sectorsManager;
            private ProtectionManager _protectionManager;

            private Settings _settings;

            private long _size;
            private Dictionary<Hash, ClusterInfo> _clusterIndex;

            private EventQueue<Hash> _addedBlockEventQueue = new EventQueue<Hash>(new TimeSpan(0, 0, 3));
            private EventQueue<Hash> _removedBlockEventQueue = new EventQueue<Hash>(new TimeSpan(0, 0, 3));

            private Omnius.Base.TimerScheduler _updateTimer;

            private readonly object _lockObject = new object();

            private volatile bool _disposed;

            public static readonly int SectorSize = 1024 * 256; // 256 KB

            public BlocksManager(string configPath, string blocksPath, BufferManager bufferManager)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
                    _fileStream = new FileStream(blocksPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, SectorSize, FileFlagNoBuffering);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _fileStream = new FileStream(blocksPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, SectorSize);
                }
                else
                {
                    throw new NotSupportedException();
                }

                _bufferManager = bufferManager;
                _sectorsManager = new SectorsManager(_bufferManager);
                _protectionManager = new ProtectionManager();

                _settings = new Settings(configPath);

                _updateTimer = new Omnius.Base.TimerScheduler(this.UpdateTimer);
            }

            private static long Roundup(long value, long unit)
            {
                if (value % unit == 0) return value;
                else return ((value / unit) + 1) * unit;
            }

            private void UpdateTimer()
            {
                this.UpdateReport();
            }

            private volatile CacheReport _report;

            private void UpdateReport()
            {
                lock (_lockObject)
                {
                    long blockCount = _clusterIndex.Count;
                    long usingSpace = _fileStream.Length;

                    long lockSpace = 0;
                    {
                        foreach (var hash in _protectionManager.GetHashes())
                        {
                            if (_clusterIndex.TryGetValue(hash, out var clusterInfo))
                            {
                                lockSpace += clusterInfo.Indexes.Length * SectorSize;
                            }
                        }
                    }

                    long freeSpace = (_size - lockSpace);

                    _report = new CacheReport(blockCount, usingSpace, lockSpace, freeSpace);
                }
            }

            public CacheReport Report
            {
                get
                {
                    return _report;
                }
            }

            public long Size
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _size;
                    }
                }
            }

            public long Count
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _clusterIndex.Count;
                    }
                }
            }

            public event Action<IEnumerable<Hash>> AddedBlockEvents
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

            public event Action<IEnumerable<Hash>> RemovedBlockEvents
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

            private IEnumerable<long> GetFreeSectors(int count)
            {
                lock (_lockObject)
                {
                    if (_sectorsManager.FreeSectorCount >= count)
                    {
                        return _sectorsManager.TakeFreeSectors(count);
                    }
                    else
                    {
                        var removePairs = _clusterIndex
                            .Where(n => !_protectionManager.Contains(n.Key))
                            .ToList();

                        removePairs.Sort((x, y) =>
                        {
                            return x.Value.UpdateTime.CompareTo(y.Value.UpdateTime);
                        });

                        foreach (var hash in removePairs.Select(n => n.Key))
                        {
                            this.Remove(hash);

                            if (_sectorsManager.FreeSectorCount >= 1024 * 4) break;
                        }

                        if (_sectorsManager.FreeSectorCount < count)
                        {
                            throw new SpaceNotFoundException();
                        }

                        return _sectorsManager.TakeFreeSectors(count);
                    }
                }
            }

            public void Lock(Hash hash)
            {
                lock (_lockObject)
                {
                    _protectionManager.Add(hash);
                }
            }

            public void Unlock(Hash hash)
            {
                lock (_lockObject)
                {
                    _protectionManager.Remove(hash);
                }
            }

            public bool Contains(Hash hash)
            {
                lock (_lockObject)
                {
                    return _clusterIndex.ContainsKey(hash);
                }
            }

            public IEnumerable<Hash> IntersectFrom(IEnumerable<Hash> collection)
            {
                lock (_lockObject)
                {
                    foreach (var hash in collection)
                    {
                        if (_clusterIndex.ContainsKey(hash))
                        {
                            yield return hash;
                        }
                    }
                }
            }

            public IEnumerable<Hash> ExceptFrom(IEnumerable<Hash> collection)
            {
                lock (_lockObject)
                {
                    foreach (var hash in collection)
                    {
                        if (!_clusterIndex.ContainsKey(hash))
                        {
                            yield return hash;
                        }
                    }
                }
            }

            public void Remove(Hash hash)
            {
                lock (_lockObject)
                {
                    if (_clusterIndex.TryGetValue(hash, out var clusterInfo))
                    {
                        _clusterIndex.Remove(hash);

                        _sectorsManager.SetFreeSectors(clusterInfo.Indexes);

                        // Event
                        _removedBlockEventQueue.Enqueue(hash);
                    }
                }
            }

            public void Resize(long size)
            {
                if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

                lock (_lockObject)
                {
                    int unit = 1024 * 1024 * 256; // 256MB
                    size = Roundup(size, unit);

                    foreach (var key in _clusterIndex.Keys.ToArray()
                        .Where(n => _clusterIndex[n].Indexes.Any(point => size < (point * SectorSize) + SectorSize))
                        .ToArray())
                    {
                        this.Remove(key);
                    }

                    _size = Roundup(size, SectorSize);
                    _fileStream.SetLength(Math.Min(_size, _fileStream.Length));

                    this.UpdateSectorsBitmap();
                }
            }

            private void UpdateSectorsBitmap()
            {
                lock (_lockObject)
                {
                    _sectorsManager.Reallocate(_size);

                    foreach (var indexes in _clusterIndex.Values.Select(n => n.Indexes))
                    {
                        _sectorsManager.SetUsingSectors(indexes);
                    }
                }
            }

            public Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
            {
                return Task.Run(() =>
                {
                    // 重複するセクタを確保したブロックを検出しRemoveする。
                    lock (_lockObject)
                    {
                        using (var bitmapManager = new BitmapManager(_bufferManager))
                        {
                            bitmapManager.SetLength(this.Size / SectorSize);

                            var hashes = new List<Hash>();

                            foreach (var (hash, clusterInfo) in _clusterIndex)
                            {
                                foreach (var sector in clusterInfo.Indexes)
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

                        int badCount = 0;
                        int checkedCount = 0;
                        int blockCount = list.Length;

                        token.ThrowIfCancellationRequested();

                        progress.Invoke(new CheckBlocksProgressReport(badCount, checkedCount, blockCount));

                        foreach (var hash in list)
                        {
                            token.ThrowIfCancellationRequested();

                            ArraySegment<byte>? block = null;

                            try
                            {
                                lock (_lockObject)
                                {
                                    if (this.Contains(hash))
                                    {
                                        block = this.Get(hash);

                                        if (block == null)
                                        {
                                            badCount++;
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                            finally
                            {
                                if (block != null)
                                {
                                    _bufferManager.ReturnBuffer(block.Value.Array);
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

            public ArraySegment<byte>? Get(Hash hash)
            {
                ArraySegment<byte> result;

                lock (_lockObject)
                {
                    ClusterInfo clusterInfo = null;

                    if (_clusterIndex.TryGetValue(hash, out clusterInfo))
                    {
                        clusterInfo.UpdateTime = DateTime.UtcNow;
                    }

                    if (clusterInfo == null) return null;

                    var buffer = _bufferManager.TakeBuffer(clusterInfo.Length);

                    try
                    {
                        for (int i = 0, remain = clusterInfo.Length; i < clusterInfo.Indexes.Length; i++, remain -= SectorSize)
                        {
                            long posision = clusterInfo.Indexes[i] * SectorSize;
                            if (posision > _fileStream.Length) throw new ArgumentOutOfRangeException();

                            if (_fileStream.Position != posision)
                            {
                                _fileStream.Seek(posision, SeekOrigin.Begin);
                            }

                            int length = Math.Min(remain, SectorSize);

                            _fileStream.Read(_sectorBuffer, 0, _sectorBuffer.Length);
                            Unsafe.Copy(_sectorBuffer, 0, buffer, SectorSize * i, length);
                        }

                        result = new ArraySegment<byte>(buffer, 0, clusterInfo.Length);
                    }
                    catch (Exception e)
                    {
                        _bufferManager.ReturnBuffer(buffer);

                        Log.Error(e);

                        return null;
                    }
                }

                if (hash.Algorithm == HashAlgorithm.Sha256)
                {
                    if (Unsafe.Equals(Sha256.Compute(result), hash.Value))
                    {
                        return result;
                    }
                    else
                    {
                        _bufferManager.ReturnBuffer(result.Array);

                        this.Remove(hash);

                        return null;
                    }
                }
                else
                {
                    throw new FormatException();
                }
            }

            public void Set(Hash hash, ArraySegment<byte> value)
            {
                if (value.Count > 1024 * 1024 * 32) throw new BadBlockException();

                if (hash.Algorithm == HashAlgorithm.Sha256)
                {
                    if (!Unsafe.Equals(Sha256.Compute(value), hash.Value)) throw new BadBlockException();
                }
                else
                {
                    throw new FormatException();
                }

                lock (_lockObject)
                {
                    if (this.Contains(hash)) return;

                    var sectorList = new List<long>();

                    try
                    {
                        sectorList.AddRange(this.GetFreeSectors((value.Count + (SectorSize - 1)) / SectorSize));

                        for (int i = 0, remain = value.Count; i < sectorList.Count && 0 < remain; i++, remain -= SectorSize)
                        {
                            long posision = sectorList[i] * SectorSize;

                            if ((_fileStream.Length < posision + SectorSize))
                            {
                                int unit = 1024 * 1024 * 256; // 256MB
                                long size = Roundup((posision + SectorSize), unit);

                                _fileStream.SetLength(Math.Min(size, this.Size));
                            }

                            if (_fileStream.Position != posision)
                            {
                                _fileStream.Seek(posision, SeekOrigin.Begin);
                            }

                            int length = Math.Min(remain, SectorSize);

                            Unsafe.Copy(value.Array, value.Offset + (SectorSize * i), _sectorBuffer, 0, length);
                            Unsafe.Zero(_sectorBuffer, length, _sectorBuffer.Length - length);

                            _fileStream.Write(_sectorBuffer, 0, _sectorBuffer.Length);
                        }

                        _fileStream.Flush();
                    }
                    catch (SpaceNotFoundException e)
                    {
                        Log.Error(e);

                        throw e;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);

                        throw e;
                    }

                    _clusterIndex[hash] = new ClusterInfo(sectorList.ToArray(), value.Count, DateTime.UtcNow);

                    // Event
                    _addedBlockEventQueue.Enqueue(hash);
                }
            }

            public int GetLength(Hash hash)
            {
                lock (_lockObject)
                {
                    if (_clusterIndex.ContainsKey(hash))
                    {
                        return _clusterIndex[hash].Length;
                    }

                    return 0;
                }
            }

            #region ISettings

            public void Load()
            {
                lock (_lockObject)
                {
                    int version = _settings.Load("Version", () => 0);

                    _size = _settings.Load("Size", () => (long)1024 * 1024 * 1024 * 32);
                    _clusterIndex = _settings.Load("ClusterIndex", () => new Dictionary<Hash, ClusterInfo>());

                    this.UpdateSectorsBitmap();

                    _updateTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(0, 1, 0));
                }
            }

            public void Save()
            {
                lock (_lockObject)
                {
                    _settings.Save("Version", 0);

                    _settings.Save("Size", _size);
                    _settings.Save("ClusterIndex", _clusterIndex);
                }
            }

            #endregion

            public Hash[] ToArray()
            {
                lock (_lockObject)
                {
                    return _clusterIndex.Keys.ToArray();
                }
            }

            #region IEnumerable<Hash>

            public IEnumerator<Hash> GetEnumerator()
            {
                lock (_lockObject)
                {
                    foreach (var hash in _clusterIndex.Keys)
                    {
                        yield return hash;
                    }
                }
            }

            #endregion

            #region IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                lock (_lockObject)
                {
                    return this.GetEnumerator();
                }
            }

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;

                if (disposing)
                {
                    _addedBlockEventQueue.Dispose();
                    _removedBlockEventQueue.Dispose();

                    if (_sectorsManager != null)
                    {
                        try
                        {
                            _sectorsManager.Dispose();
                        }
                        catch (Exception)
                        {

                        }

                        _sectorsManager = null;
                    }

                    if (_fileStream != null)
                    {
                        try
                        {
                            _fileStream.Dispose();
                        }
                        catch (Exception)
                        {

                        }

                        _fileStream = null;
                    }

                    if (_updateTimer != null)
                    {
                        try
                        {
                            _updateTimer.Dispose();
                        }
                        catch (Exception)
                        {

                        }

                        _updateTimer = null;
                    }
                }
            }
        }
    }
}
