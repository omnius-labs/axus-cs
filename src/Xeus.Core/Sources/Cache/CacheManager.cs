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
    sealed partial class CacheManager : DisposableBase, ISettings, ISetOperators<Hash>, IEnumerable<Hash>
    {
        private BufferManager _bufferManager;
        private BlocksManager _blocksManager;
        private ContentInfosManager _contentInfoManager;

        private Settings _settings;

        private Omnius.Base.TimerScheduler _checkTimer;

        private EventQueue<Hash> _removedBlockEventQueue = new EventQueue<Hash>(new TimeSpan(0, 0, 3));

        private readonly object _lockObject = new object();

        private volatile bool _disposed;

        private readonly int _threadCount = 2;

        public CacheManager(string configPath, string blocksPath, BufferManager bufferManager)
        {
            _bufferManager = bufferManager;
            _blocksManager = new BlocksManager(Path.Combine(configPath, "Blocks"), blocksPath, _bufferManager);
            _contentInfoManager = new ContentInfosManager();

            _settings = new Settings(configPath);

            _checkTimer = new Omnius.Base.TimerScheduler(this.CheckTimer);
        }

        private void CheckTimer()
        {
            this.CheckMessages();
            this.CheckContents();
        }

        public CacheReport Report
        {
            get
            {
                return _blocksManager.Report;
            }
        }

        public IEnumerable<CacheContentReport> GetCacheContentReports()
        {
            lock (_lockObject)
            {
                var list = new List<CacheContentReport>();

                foreach (var info in _contentInfoManager.GetFileContentInfos())
                {
                    list.Add(new CacheContentReport(info.CreationTime, info.ShareInfo.FileLength, info.Metadata, info.ShareInfo.Path));
                }

                return list;
            }
        }

        public long Size
        {
            get
            {
                return _blocksManager.Size;
            }
        }

        public event Action<IEnumerable<Hash>> AddedBlockEvents
        {
            add
            {
                _blocksManager.AddedBlockEvents += value;
            }
            remove
            {
                _blocksManager.AddedBlockEvents -= value;
            }
        }

        public event Action<IEnumerable<Hash>> RemovedBlockEvents
        {
            add
            {
                _blocksManager.RemovedBlockEvents += value;
                _removedBlockEventQueue.Events += value;
            }
            remove
            {
                _blocksManager.RemovedBlockEvents -= value;
                _removedBlockEventQueue.Events -= value;
            }
        }

        public void Lock(Hash hash)
        {
            _blocksManager.Lock(hash);
        }

        public void Unlock(Hash hash)
        {
            _blocksManager.Unlock(hash);
        }

        public bool Contains(Hash hash)
        {
            if (_blocksManager.Contains(hash)) return true;

            lock (_lockObject)
            {
                if (_contentInfoManager.Contains(hash)) return true;
            }

            return false;
        }

        public IEnumerable<Hash> IntersectFrom(IEnumerable<Hash> collection)
        {
            var hashSet = new HashSet<Hash>();
            hashSet.UnionWith(_blocksManager.IntersectFrom(collection));

            lock (_lockObject)
            {
                hashSet.UnionWith(_contentInfoManager.IntersectFrom(collection));
            }

            return hashSet;
        }

        public IEnumerable<Hash> ExceptFrom(IEnumerable<Hash> collection)
        {
            var hashSet = new HashSet<Hash>(collection);
            hashSet.ExceptWith(_blocksManager.IntersectFrom(collection));

            lock (_lockObject)
            {
                hashSet.ExceptWith(_contentInfoManager.IntersectFrom(collection));
            }

            return hashSet;
        }

        public void Resize(long size)
        {
            _blocksManager.Resize(size);
        }

        public Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            return _blocksManager.CheckBlocks(progress, token);
        }

        public ArraySegment<byte> GetBlock(Hash hash)
        {
            // Cache
            {
                var result = _blocksManager.Get(hash);

                if (result != null)
                {
                    return result.Value;
                }
            }

            // Share
            {
                ArraySegment<byte>? result = null;
                string path = null;

                lock (_lockObject)
                {
                    var shareInfo = _contentInfoManager.GetShareInfo(hash);

                    if (shareInfo != null)
                    {
                        var buffer = _bufferManager.TakeBuffer(shareInfo.BlockLength);

                        try
                        {
                            int length;

                            try
                            {
                                using (var stream = new UnbufferedFileStream(shareInfo.Path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, _bufferManager))
                                {
                                    stream.Seek((long)shareInfo.GetIndex(hash) * shareInfo.BlockLength, SeekOrigin.Begin);

                                    length = (int)Math.Min(stream.Length - stream.Position, shareInfo.BlockLength);
                                    stream.Read(buffer, 0, length);
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                throw new BlockNotFoundException();
                            }
                            catch (IOException)
                            {
                                throw new BlockNotFoundException();
                            }

                            result = new ArraySegment<byte>(buffer, 0, length);
                            path = shareInfo.Path;
                        }
                        catch (Exception)
                        {
                            _bufferManager.ReturnBuffer(buffer);

                            throw;
                        }
                    }
                }

                if (result != null)
                {
                    if (hash.Algorithm == HashAlgorithm.Sha256
                        && Unsafe.Equals(Sha256.Compute(result.Value), hash.Value))
                    {
                        return result.Value;
                    }
                    else
                    {
                        _bufferManager.ReturnBuffer(result.Value.Array);
                        result = null;

                        this.RemoveContent(path);
                    }
                }
            }

            throw new BlockNotFoundException();
        }

        public void Set(Hash hash, ArraySegment<byte> value)
        {
            _blocksManager.Set(hash, value);
        }

        public int GetLength(Hash hash)
        {
            // Cache
            {
                int length = _blocksManager.GetLength(hash);
                if (length != 0) return length;
            }

            // Share
            {
                lock (_lockObject)
                {
                    var shareInfo = _contentInfoManager.GetShareInfo(hash);

                    if (shareInfo != null)
                    {
                        return (int)Math.Min(shareInfo.FileLength - ((long)shareInfo.BlockLength * shareInfo.GetIndex(hash)), shareInfo.BlockLength);
                    }
                }
            }

            return 0;
        }

        public Stream Decoding(IEnumerable<Hash> hashes)
        {
            if (hashes == null) throw new ArgumentNullException(nameof(hashes));

            return new CacheStreamReader(hashes.ToList(), this, _bufferManager);
        }

        public Task<IEnumerable<Hash>> ParityDecoding(Group group, CancellationToken token)
        {
            return Task.Run<IEnumerable<Hash>>(() =>
            {
                if (group.CorrectionAlgorithm == CorrectionAlgorithm.ReedSolomon8)
                {
                    int blockLength = group.Hashes.Max(n => this.GetLength(n));
                    int informationCount = group.Hashes.Count / 2;

                    if (group.Hashes.Take(informationCount).All(n => this.Contains(n)))
                    {
                        return group.Hashes.Take(informationCount).ToList();
                    }

                    var buffers = new ArraySegment<byte>[informationCount];
                    var indexes = new int[informationCount];

                    try
                    {
                        // Load
                        {
                            int count = 0;

                            for (int i = 0; i < group.Hashes.Count; i++)
                            {
                                token.ThrowIfCancellationRequested();

                                if (!this.Contains(group.Hashes[i])) continue;

                                var buffer = new ArraySegment<byte>();

                                try
                                {
                                    buffer = this.GetBlock(group.Hashes[i]);

                                    if (buffer.Count < blockLength)
                                    {
                                        var tempBuffer = new ArraySegment<byte>(_bufferManager.TakeBuffer(blockLength), 0, blockLength);
                                        Unsafe.Copy(buffer.Array, buffer.Offset, tempBuffer.Array, tempBuffer.Offset, buffer.Count);
                                        Unsafe.Zero(tempBuffer.Array, tempBuffer.Offset + buffer.Count, tempBuffer.Count - buffer.Count);

                                        _bufferManager.ReturnBuffer(buffer.Array);

                                        buffer = tempBuffer;
                                    }
                                }
                                catch (Exception)
                                {
                                    if (buffer.Array != null)
                                    {
                                        _bufferManager.ReturnBuffer(buffer.Array);
                                    }

                                    throw;
                                }

                                indexes[count] = i;
                                buffers[count] = buffer;

                                count++;

                                if (count >= informationCount) break;
                            }

                            if (count < informationCount) throw new BlockNotFoundException();
                        }

                        using (var reedSolomon = new ReedSolomon8(informationCount, informationCount * 2, _threadCount, _bufferManager))
                        {
                            reedSolomon.Decode(buffers, indexes, blockLength, token).Wait();
                        }

                        // Set
                        {
                            long length = group.Length;

                            for (int i = 0; i < informationCount; length -= blockLength, i++)
                            {
                                _blocksManager.Set(group.Hashes[i], new ArraySegment<byte>(buffers[i].Array, buffers[i].Offset, (int)Math.Min(buffers[i].Count, length)));
                            }
                        }
                    }
                    finally
                    {
                        foreach (var buffer in buffers)
                        {
                            if (buffer.Array == null) continue;

                            _bufferManager.ReturnBuffer(buffer.Array);
                        }
                    }

                    return group.Hashes.Take(informationCount).ToList();
                }
                else
                {
                    throw new NotSupportedException();
                }
            });
        }

        public Task<Metadata> Import(Stream stream, TimeSpan lifeSpan, CancellationToken token)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return Task.Run(() =>
            {
                Metadata metadata = null;
                var lockedHashes = new HashSet<Hash>();

                try
                {
                    const int blockLength = 1024 * 1024;
                    const HashAlgorithm hashAlgorithm = HashAlgorithm.Sha256;
                    const CorrectionAlgorithm correctionAlgorithm = CorrectionAlgorithm.ReedSolomon8;

                    int depth = 0;
                    var creationTime = DateTime.UtcNow;

                    var groupList = new List<Group>();

                    for (; ; )
                    {
                        if (stream.Length <= blockLength)
                        {
                            Hash hash;

                            using (var safeBuffer = _bufferManager.CreateSafeBuffer(blockLength))
                            {
                                int length = (int)stream.Length;
                                stream.Read(safeBuffer.Value, 0, length);

                                if (hashAlgorithm == HashAlgorithm.Sha256)
                                {
                                    hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(safeBuffer.Value, 0, length));
                                }

                                _blocksManager.Lock(hash);
                                _blocksManager.Set(hash, new ArraySegment<byte>(safeBuffer.Value, 0, length));

                                lockedHashes.Add(hash);
                            }

                            // Stream Dispose
                            {
                                stream.Dispose();
                                stream = null;
                            }

                            metadata = new Metadata(depth, hash);

                            break;
                        }
                        else
                        {
                            for (; ; )
                            {
                                var targetHashes = new List<Hash>();
                                var targetBuffers = new List<ArraySegment<byte>>();
                                long sumLength = 0;

                                try
                                {
                                    for (int i = 0; stream.Position < stream.Length; i++)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        var buffer = new ArraySegment<byte>();

                                        try
                                        {
                                            int length = (int)Math.Min(stream.Length - stream.Position, blockLength);
                                            buffer = new ArraySegment<byte>(_bufferManager.TakeBuffer(length), 0, length);
                                            stream.Read(buffer.Array, 0, length);

                                            sumLength += length;
                                        }
                                        catch (Exception)
                                        {
                                            if (buffer.Array != null)
                                            {
                                                _bufferManager.ReturnBuffer(buffer.Array);
                                            }

                                            throw;
                                        }

                                        Hash hash;

                                        if (hashAlgorithm == HashAlgorithm.Sha256)
                                        {
                                            hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(buffer));
                                        }

                                        _blocksManager.Lock(hash);
                                        _blocksManager.Set(hash, buffer);

                                        lockedHashes.Add(hash);

                                        targetHashes.Add(hash);
                                        targetBuffers.Add(buffer);

                                        if (targetBuffers.Count >= 128) break;
                                    }

                                    var parityHashes = this.ParityEncoding(targetBuffers, hashAlgorithm, correctionAlgorithm, token);
                                    lockedHashes.UnionWith(parityHashes);

                                    groupList.Add(new Group(correctionAlgorithm, sumLength, CollectionUtils.Unite(targetHashes, parityHashes).ToArray()));
                                }
                                finally
                                {
                                    foreach (var buffer in targetBuffers)
                                    {
                                        if (buffer.Array == null) continue;

                                        _bufferManager.ReturnBuffer(buffer.Array);
                                    }
                                }

                                if (stream.Position == stream.Length) break;
                            }

                            depth++;

                            // Stream Dispose
                            {
                                stream.Dispose();
                                stream = null;
                            }

                            stream = (new Index(groupList)).Export(_bufferManager);
                        }
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }
                }

                lock (_lockObject)
                {
                    if (!_contentInfoManager.ContainsMessageContentInfo(metadata))
                    {
                        _contentInfoManager.Add(new ContentInfo(DateTime.UtcNow, lifeSpan, metadata, lockedHashes, null));

                        foreach (var hash in lockedHashes)
                        {
                            _blocksManager.Lock(hash);
                        }
                    }
                }

                return metadata;
            }, token);
        }

        public Task<Metadata> Import(string path, DateTime creationTime, CancellationToken token)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return Task.Run(() =>
            {
                // Check
                lock (_lockObject)
                {
                    var info = _contentInfoManager.GetFileContentInfo(path);
                    if (info != null) return info.Metadata;
                }

                Metadata metadata = null;
                var lockedHashes = new HashSet<Hash>();
                ShareInfo shareInfo = null;

                {
                    const int blockLength = 1024 * 1024;
                    const HashAlgorithm hashAlgorithm = HashAlgorithm.Sha256;
                    const CorrectionAlgorithm correctionAlgorithm = CorrectionAlgorithm.ReedSolomon8;

                    int depth = 0;

                    var groupList = new List<Group>();

                    // File
                    using (var stream = new UnbufferedFileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, _bufferManager))
                    {
                        if (stream.Length <= blockLength)
                        {
                            Hash hash;

                            using (var safeBuffer = _bufferManager.CreateSafeBuffer(blockLength))
                            {
                                int length = (int)stream.Length;
                                stream.Read(safeBuffer.Value, 0, length);

                                if (hashAlgorithm == HashAlgorithm.Sha256)
                                {
                                    hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(safeBuffer.Value, 0, length));
                                }
                            }

                            shareInfo = new ShareInfo(path, stream.Length, (int)stream.Length, new Hash[] { hash });
                            metadata = new Metadata(depth, hash);
                        }
                        else
                        {
                            var sharedHashes = new List<Hash>();

                            for (; ; )
                            {
                                var targetHashes = new List<Hash>();
                                var targetBuffers = new List<ArraySegment<byte>>();
                                long sumLength = 0;

                                try
                                {
                                    for (int i = 0; stream.Position < stream.Length; i++)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        var buffer = new ArraySegment<byte>();

                                        try
                                        {
                                            int length = (int)Math.Min(stream.Length - stream.Position, blockLength);
                                            buffer = new ArraySegment<byte>(_bufferManager.TakeBuffer(length), 0, length);
                                            stream.Read(buffer.Array, 0, length);

                                            sumLength += length;
                                        }
                                        catch (Exception)
                                        {
                                            if (buffer.Array != null)
                                            {
                                                _bufferManager.ReturnBuffer(buffer.Array);
                                            }

                                            throw;
                                        }

                                        Hash hash;

                                        if (hashAlgorithm == HashAlgorithm.Sha256)
                                        {
                                            hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(buffer));
                                        }

                                        sharedHashes.Add(hash);

                                        targetHashes.Add(hash);
                                        targetBuffers.Add(buffer);

                                        if (targetBuffers.Count >= 128) break;
                                    }

                                    var parityHashes = this.ParityEncoding(targetBuffers, hashAlgorithm, correctionAlgorithm, token);
                                    lockedHashes.UnionWith(parityHashes);

                                    groupList.Add(new Group(correctionAlgorithm, sumLength, CollectionUtils.Unite(targetHashes, parityHashes).ToArray()));
                                }
                                finally
                                {
                                    foreach (var buffer in targetBuffers)
                                    {
                                        if (buffer.Array == null) continue;

                                        _bufferManager.ReturnBuffer(buffer.Array);
                                    }
                                }

                                if (stream.Position == stream.Length) break;
                            }

                            shareInfo = new ShareInfo(path, stream.Length, blockLength, sharedHashes);

                            depth++;
                        }
                    }

                    while (groupList.Count > 0)
                    {
                        // Index
                        using (var stream = (new Index(groupList)).Export(_bufferManager))
                        {
                            groupList.Clear();

                            if (stream.Length <= blockLength)
                            {
                                Hash hash;

                                using (var safeBuffer = _bufferManager.CreateSafeBuffer(blockLength))
                                {
                                    int length = (int)stream.Length;
                                    stream.Read(safeBuffer.Value, 0, length);

                                    if (hashAlgorithm == HashAlgorithm.Sha256)
                                    {
                                        hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(safeBuffer.Value, 0, length));
                                    }

                                    _blocksManager.Lock(hash);
                                    _blocksManager.Set(hash, new ArraySegment<byte>(safeBuffer.Value, 0, length));

                                    lockedHashes.Add(hash);
                                }

                                metadata = new Metadata(depth, hash);
                            }
                            else
                            {
                                for (; ; )
                                {
                                    var targetHashes = new List<Hash>();
                                    var targetBuffers = new List<ArraySegment<byte>>();
                                    long sumLength = 0;

                                    try
                                    {
                                        for (int i = 0; stream.Position < stream.Length; i++)
                                        {
                                            token.ThrowIfCancellationRequested();

                                            var buffer = new ArraySegment<byte>();

                                            try
                                            {
                                                int length = (int)Math.Min(stream.Length - stream.Position, blockLength);
                                                buffer = new ArraySegment<byte>(_bufferManager.TakeBuffer(length), 0, length);
                                                stream.Read(buffer.Array, 0, length);

                                                sumLength += length;
                                            }
                                            catch (Exception)
                                            {
                                                if (buffer.Array != null)
                                                {
                                                    _bufferManager.ReturnBuffer(buffer.Array);
                                                }

                                                throw;
                                            }

                                            Hash hash;

                                            if (hashAlgorithm == HashAlgorithm.Sha256)
                                            {
                                                hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(buffer));
                                            }

                                            _blocksManager.Lock(hash);
                                            _blocksManager.Set(hash, buffer);

                                            lockedHashes.Add(hash);

                                            targetHashes.Add(hash);
                                            targetBuffers.Add(buffer);

                                            if (targetBuffers.Count >= 128) break;
                                        }

                                        var parityHashes = this.ParityEncoding(targetBuffers, hashAlgorithm, correctionAlgorithm, token);
                                        lockedHashes.UnionWith(parityHashes);

                                        groupList.Add(new Group(correctionAlgorithm, sumLength, CollectionUtils.Unite(targetHashes, parityHashes).ToArray()));
                                    }
                                    finally
                                    {
                                        foreach (var buffer in targetBuffers)
                                        {
                                            if (buffer.Array == null) continue;

                                            _bufferManager.ReturnBuffer(buffer.Array);
                                        }
                                    }

                                    if (stream.Position == stream.Length) break;
                                }

                                depth++;
                            }
                        }
                    }
                }

                lock (_lockObject)
                {
                    if (!_contentInfoManager.ContainsFileContentInfo(path))
                    {
                        _contentInfoManager.Add(new ContentInfo(creationTime, Timeout.InfiniteTimeSpan, metadata, lockedHashes, shareInfo));

                        foreach (var hash in lockedHashes)
                        {
                            _blocksManager.Lock(hash);
                        }
                    }
                }

                return metadata;
            }, token);
        }

        private IEnumerable<Hash> ParityEncoding(IEnumerable<ArraySegment<byte>> buffers, HashAlgorithm hashAlgorithm, CorrectionAlgorithm correctionAlgorithm, CancellationToken token)
        {
            if (correctionAlgorithm == CorrectionAlgorithm.ReedSolomon8)
            {
                if (buffers.Count() > 128) throw new ArgumentOutOfRangeException(nameof(buffers));

                var createBuffers = new List<ArraySegment<byte>>();

                try
                {
                    var targetBuffers = new ArraySegment<byte>[buffers.Count()];
                    var parityBuffers = new ArraySegment<byte>[buffers.Count()];

                    int blockLength = buffers.Max(n => n.Count);

                    // Normalize
                    {
                        int index = 0;

                        foreach (var buffer in buffers)
                        {
                            token.ThrowIfCancellationRequested();

                            if (buffer.Count < blockLength)
                            {
                                var tempBuffer = new ArraySegment<byte>(_bufferManager.TakeBuffer(blockLength), 0, blockLength);
                                Unsafe.Copy(buffer.Array, buffer.Offset, tempBuffer.Array, tempBuffer.Offset, buffer.Count);
                                Unsafe.Zero(tempBuffer.Array, tempBuffer.Offset + buffer.Count, tempBuffer.Count - buffer.Count);

                                createBuffers.Add(tempBuffer);

                                targetBuffers[index] = tempBuffer;
                            }
                            else
                            {
                                targetBuffers[index] = buffer;
                            }

                            index++;
                        }
                    }

                    for (int i = 0; i < parityBuffers.Length; i++)
                    {
                        parityBuffers[i] = new ArraySegment<byte>(_bufferManager.TakeBuffer(blockLength), 0, blockLength);
                    }

                    var indexes = new int[parityBuffers.Length];

                    for (int i = 0; i < parityBuffers.Length; i++)
                    {
                        indexes[i] = targetBuffers.Length + i;
                    }

                    using (var reedSolomon = new ReedSolomon8(targetBuffers.Length, targetBuffers.Length + parityBuffers.Length, _threadCount, _bufferManager))
                    {
                        reedSolomon.Encode(targetBuffers, parityBuffers, indexes, blockLength, token).Wait();
                    }

                    token.ThrowIfCancellationRequested();

                    var parityHashes = new List<Hash>();

                    for (int i = 0; i < parityBuffers.Length; i++)
                    {
                        Hash hash;

                        if (hashAlgorithm == HashAlgorithm.Sha256)
                        {
                            hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(parityBuffers[i]));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        _blocksManager.Lock(hash);
                        _blocksManager.Set(hash, parityBuffers[i]);

                        parityHashes.Add(hash);
                    }

                    return parityHashes;
                }
                finally
                {
                    foreach (var buffer in createBuffers)
                    {
                        if (buffer.Array == null) continue;

                        _bufferManager.ReturnBuffer(buffer.Array);
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #region Message

        private void CheckMessages()
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                foreach (var info in _contentInfoManager.GetMessageContentInfos())
                {
                    if ((now - info.CreationTime) > info.LifeSpan)
                    {
                        _contentInfoManager.RemoveMessageContentInfo(info.Metadata);

                        foreach (var hash in info.LockedHashes)
                        {
                            _blocksManager.Unlock(hash);
                        }
                    }
                }
            }
        }

        #endregion

        #region Content

        private void CheckContents()
        {
            lock (_lockObject)
            {
                foreach (var cacheInfo in _contentInfoManager.GetFileContentInfos())
                {
                    if (cacheInfo.LockedHashes.All(n => this.Contains(n))) continue;

                    this.RemoveContent(cacheInfo.ShareInfo.Path);
                }
            }
        }

        public void RemoveContent(string path)
        {
            lock (_lockObject)
            {
                var contentInfo = _contentInfoManager.GetFileContentInfo(path);
                if (contentInfo == null) return;

                _contentInfoManager.RemoveFileContentInfo(path);

                foreach (var hash in contentInfo.LockedHashes)
                {
                    _blocksManager.Unlock(hash);
                }

                // Event
                _removedBlockEventQueue.Enqueue(contentInfo.ShareInfo.Hashes.Where(n => !this.Contains(n)).ToArray());
            }
        }

        public IEnumerable<Hash> GetContentHashes(string path)
        {
            lock (_lockObject)
            {
                var contentInfo = _contentInfoManager.GetFileContentInfo(path);
                if (contentInfo == null) Enumerable.Empty<Hash>();

                return contentInfo.LockedHashes.ToArray();
            }
        }

        #endregion

        #region ISettings

        public void Load()
        {
            lock (_lockObject)
            {
                _blocksManager.Load();

                int version = _settings.Load("Version", () => 0);

                foreach (var contentInfo in _settings.Load<IEnumerable<ContentInfo>>("ContentInfos", () => Array.Empty<ContentInfo>()))
                {
                    _contentInfoManager.Add(contentInfo);

                    foreach (var hash in contentInfo.LockedHashes)
                    {
                        _blocksManager.Lock(hash);
                    }
                }

                _checkTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(0, 10, 0));
            }
        }

        public void Save()
        {
            lock (_lockObject)
            {
                _blocksManager.Save();

                _settings.Save("Version", 0);

                _settings.Save("ContentInfos", _contentInfoManager.ToArray());
            }
        }

        #endregion

        public Hash[] ToArray()
        {
            lock (_lockObject)
            {
                var hashSet = new HashSet<Hash>();
                hashSet.UnionWith(_blocksManager.ToArray());
                hashSet.UnionWith(_contentInfoManager.GetHashes());

                return hashSet.ToArray();
            }
        }

        #region IEnumerable<Hash>

        public IEnumerator<Hash> GetEnumerator()
        {
            lock (_lockObject)
            {
                foreach (var hash in this.ToArray())
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
                _removedBlockEventQueue.Dispose();

                if (_blocksManager != null)
                {
                    try
                    {
                        _blocksManager.Dispose();
                    }
                    catch (Exception)
                    {

                    }

                    _blocksManager = null;
                }

                if (_checkTimer != null)
                {
                    try
                    {
                        _checkTimer.Dispose();
                    }
                    catch (Exception)
                    {

                    }

                    _checkTimer = null;
                }

                if (_checkTimer != null)
                {
                    try
                    {
                        _checkTimer.Dispose();
                    }
                    catch (Exception)
                    {

                    }

                    _checkTimer = null;
                }
            }
        }
    }

    class CacheManagerException : ManagerException
    {
        public CacheManagerException() : base() { }
        public CacheManagerException(string message) : base(message) { }
        public CacheManagerException(string message, Exception innerException) : base(message, innerException) { }
    }

    class SpaceNotFoundException : CacheManagerException
    {
        public SpaceNotFoundException() : base() { }
        public SpaceNotFoundException(string message) : base(message) { }
        public SpaceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    class BlockNotFoundException : CacheManagerException
    {
        public BlockNotFoundException() : base() { }
        public BlockNotFoundException(string message) : base(message) { }
        public BlockNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    class BadBlockException : CacheManagerException
    {
        public BadBlockException() : base() { }
        public BadBlockException(string message) : base(message) { }
        public BadBlockException(string message, Exception innerException) : base(message, innerException) { }
    }
}
