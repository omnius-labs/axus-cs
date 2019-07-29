using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Algorithms.Correction;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Configuration;
using Omnix.Io;
using Omnix.Serialization.RocketPack.Helpers;
using Xeus.Core.Internal.Content.Primitives;
using Xeus.Core.Internal.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Internal.Content
{
    internal sealed class ContentStorage : DisposableBase, ISettings, ISetOperators<OmniHash>, IEnumerable<OmniHash>
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusOptions _options;
        private readonly BufferPool _bufferPool;
        private readonly BlockStorage _blockStorage;
        private readonly ContentMetadataStorage _contentMetadataStorage;

        private readonly SettingsDatabase _settings;

        private readonly EventScheduler _checkEventScheduler;

        private readonly LazyEvent<OmniHash> _removedBlockEventQueue = new LazyEvent<OmniHash>(new TimeSpan(0, 0, 3));

        private readonly object _lockObject = new object();

        private readonly int _threadCount = 2;

        public ContentStorage(XeusOptions options, BufferPool bufferPool)
        {
            _options = options;
            _bufferPool = bufferPool;
            _blockStorage = new BlockStorage(options, _bufferPool);
            _contentMetadataStorage = new ContentMetadataStorage();

            string configPath = Path.Combine(options.ConfigDirectoryPath, nameof(ContentStorage));
            _settings = new SettingsDatabase(Path.Combine(configPath, "settings"));

            _checkEventScheduler = new EventScheduler(this.CheckThread);
        }

        private async ValueTask CheckThread(CancellationToken token)
        {
            this.CheckMessages();
            this.CheckContents();
        }

        public ulong Size
        {
            get
            {
                return _blockStorage.Size;
            }
        }

        public event Action<IEnumerable<OmniHash>> AddedBlockEvents
        {
            add
            {
                _blockStorage.AddedBlockEvents += value;
            }
            remove
            {
                _blockStorage.AddedBlockEvents -= value;
            }
        }

        public event Action<IEnumerable<OmniHash>> RemovedBlockEvents
        {
            add
            {
                _blockStorage.RemovedBlockEvents += value;
                _removedBlockEventQueue.Events += value;
            }
            remove
            {
                _blockStorage.RemovedBlockEvents -= value;
                _removedBlockEventQueue.Events -= value;
            }
        }

        public void Lock(OmniHash hash)
        {
            _blockStorage.Lock(hash);
        }

        public void Unlock(OmniHash hash)
        {
            _blockStorage.Unlock(hash);
        }

        public bool Contains(OmniHash hash)
        {
            if (_blockStorage.Contains(hash))
            {
                return true;
            }

            lock (_lockObject)
            {
                if (_contentMetadataStorage.Contains(hash))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            var hashSet = new HashSet<OmniHash>();
            hashSet.UnionWith(_blockStorage.IntersectFrom(collection));

            lock (_lockObject)
            {
                hashSet.UnionWith(_contentMetadataStorage.IntersectFrom(collection));
            }

            return hashSet;
        }

        public IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection)
        {
            var hashSet = new HashSet<OmniHash>(collection);
            hashSet.ExceptWith(_blockStorage.IntersectFrom(collection));

            lock (_lockObject)
            {
                hashSet.ExceptWith(_contentMetadataStorage.IntersectFrom(collection));
            }

            return hashSet;
        }

        public void Resize(ulong size)
        {
            _blockStorage.Resize(size);
        }

        public async ValueTask CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            await _blockStorage.CheckBlocks(progress, token);
        }

        public bool TryGetBlock(OmniHash hash, out IMemoryOwner<byte>? memoryOwner)
        {
            if (!EnumHelper.IsValid(hash.AlgorithmType))
            {
                throw new ArgumentException($"Incorrect HashAlgorithmType: {hash.AlgorithmType}");
            }

            // Cache
            {
                var result = _blockStorage.TryGet(hash, out memoryOwner);

                if (result)
                {
                    return true;
                }
            }

            bool success = false;
            string? path = null;

            // Share
            try
            {

                lock (_lockObject)
                {
                    var sharedBlocksInfo = _contentMetadataStorage.GetSharedBlocksInfo(hash);

                    if (sharedBlocksInfo != null)
                    {
                        ulong position = (ulong)sharedBlocksInfo.GetIndex(hash) * sharedBlocksInfo.BlockLength;
                        uint length = (uint)Math.Min(sharedBlocksInfo.Length - position, sharedBlocksInfo.BlockLength);

                        memoryOwner = _bufferPool.Rent((int)length);

                        try
                        {
                            using (var stream = new UnbufferedFileStream(sharedBlocksInfo.Path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, _bufferPool))
                            {
                                stream.Seek((long)position, SeekOrigin.Begin);
                                stream.Read(memoryOwner.Memory.Span);
                            }

                            path = sharedBlocksInfo.Path;
                        }
                        catch (Exception e)
                        {
                            _logger.Debug(e);

                            return false;
                        }
                    }
                }

                if (memoryOwner == null)
                {
                    return false;
                }

                if (hash.AlgorithmType == OmniHashAlgorithmType.Sha2_256
                    && BytesOperations.SequenceEqual(Sha2_256.ComputeHash(memoryOwner.Memory.Span), hash.Value.Span))
                {
                    success = true;

                    return true;
                }
                else
                {
                    _logger.Warn("Broken block.");

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

                    if (path != null)
                    {
                        this.RemoveContent(path);
                    }
                }
            }
        }

        public bool TrySetBlock(OmniHash hash, ReadOnlySpan<byte> value)
        {
            return _blockStorage.TrySet(hash, value);
        }

        public uint GetLength(OmniHash hash)
        {
            // Cache
            {
                uint length = _blockStorage.GetLength(hash);
                if (length != 0)
                {
                    return length;
                }
            }

            // Share
            {
                lock (_lockObject)
                {
                    var sharedBlocksInfo = _contentMetadataStorage.GetSharedBlocksInfo(hash);

                    if (sharedBlocksInfo != null)
                    {
                        return (uint)Math.Min(sharedBlocksInfo.Length - ((ulong)sharedBlocksInfo.BlockLength * (uint)sharedBlocksInfo.GetIndex(hash)), sharedBlocksInfo.BlockLength);
                    }
                }
            }

            return 0;
        }

        public async ValueTask<OmniHash[]> ParityDecode(MerkleTreeSection merkleTreeSection, CancellationToken token = default)
        {
            return await Task.Run(async () =>
            {
                if (merkleTreeSection.CorrectionAlgorithmType == CorrectionAlgorithmType.ReedSolomon8)
                {
                    uint blockLength = merkleTreeSection.Hashes.Max(n => this.GetLength(n));
                    int informationCount = merkleTreeSection.Hashes.Count / 2;

                    if (merkleTreeSection.Hashes.Take(informationCount).All(n => this.Contains(n)))
                    {
                        return merkleTreeSection.Hashes.Take(informationCount).ToArray();
                    }

                    var blockMemoryOwners = new IMemoryOwner<byte>[informationCount];
                    var indexes = new int[informationCount];

                    try
                    {
                        // Load
                        {
                            int count = 0;

                            for (int i = 0; i < merkleTreeSection.Hashes.Count; i++)
                            {
                                token.ThrowIfCancellationRequested();

                                if (!this.TryGetBlock(merkleTreeSection.Hashes[i], out var blockMemoryOwner) || blockMemoryOwner == null)
                                {
                                    continue;
                                }

                                if (blockMemoryOwner.Memory.Length < blockLength)
                                {
                                    var tempMemoryOwner = _bufferPool.Rent((int)blockLength);

                                    BytesOperations.Copy(blockMemoryOwner.Memory.Span, tempMemoryOwner.Memory.Span, blockMemoryOwner.Memory.Length);
                                    BytesOperations.Zero(tempMemoryOwner.Memory.Span.Slice(blockMemoryOwner.Memory.Length));

                                    blockMemoryOwner.Dispose();
                                    blockMemoryOwner = tempMemoryOwner;
                                }

                                indexes[count] = i;
                                blockMemoryOwners[count] = blockMemoryOwner;

                                count++;

                                if (count >= informationCount)
                                {
                                    break;
                                }
                            }

                            if (count < informationCount)
                            {
                                throw new BlockNotFoundException();
                            }
                        }

                        var reedSolomon = new ReedSolomon8(informationCount, _threadCount, _bufferPool);
                        await reedSolomon.Decode(blockMemoryOwners.Select(n => n.Memory).ToArray(), indexes, (int)blockLength, informationCount * 2, token);

                        // Set
                        {
                            ulong length = merkleTreeSection.Length;

                            for (int i = 0; i < informationCount; length -= blockLength, i++)
                            {
                                bool result = _blockStorage.TrySet(merkleTreeSection.Hashes[i], blockMemoryOwners[i].Memory.Span.Slice(0, (int)Math.Min(length, blockLength)));

                                if (!result)
                                {
                                    throw new ParityDecodeFailed("Failed to save Block.");
                                }
                            }
                        }
                    }
                    finally
                    {
                        foreach (var memoryOwner in blockMemoryOwners)
                        {
                            memoryOwner.Dispose();
                        }
                    }

                    return merkleTreeSection.Hashes.Take(informationCount).ToArray();
                }
                else
                {
                    throw new NotSupportedException();
                }
            });
        }

        public async ValueTask<XeusClue> Import(Stream stream, CancellationToken token = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return await Task.Run(async () =>
            {
                XeusClue? clue = null;
                var lockedHashes = new HashSet<OmniHash>();

                try
                {
                    const uint blockLength = 1024 * 1024;
                    const OmniHashAlgorithmType hashAlgorithmType = OmniHashAlgorithmType.Sha2_256;
                    const CorrectionAlgorithmType correctionAlgorithmType = CorrectionAlgorithmType.ReedSolomon8;

                    byte depth = 0;
                    var creationTime = DateTime.UtcNow;

                    var merkleTreeSectionList = new List<MerkleTreeSection>();

                    for (; ; )
                    {
                        if (stream.Length <= blockLength)
                        {
                            OmniHash hash;

                            using (var bufferMemoryOwner = _bufferPool.Rent((int)blockLength))
                            {
                                stream.Read(bufferMemoryOwner.Memory.Span);

                                if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                {
                                    hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                }

                                _blockStorage.Lock(hash);
                                lockedHashes.Add(hash);

                                bool result = _blockStorage.TrySet(hash, bufferMemoryOwner.Memory.Span);

                                if (!result)
                                {
                                    throw new ImportFailed("Failed to save Block.");
                                }
                            }

                            stream.Dispose();

                            clue = new XeusClue(hash, depth);

                            break;
                        }
                        else
                        {
                            for (; ; )
                            {
                                var targetHashes = new List<OmniHash>();
                                var targetMemoryOwners = new List<IMemoryOwner<byte>>();
                                ulong sumLength = 0;

                                try
                                {
                                    for (int i = 0; stream.Position < stream.Length; i++)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        uint length = (uint)Math.Min(stream.Length - stream.Position, blockLength);
                                        var bufferMemoryOwner = _bufferPool.Rent((int)length);

                                        try
                                        {
                                            stream.Read(bufferMemoryOwner.Memory.Span);

                                            sumLength += length;
                                        }
                                        catch (Exception e)
                                        {
                                            bufferMemoryOwner.Dispose();

                                            throw e;
                                        }

                                        OmniHash hash;

                                        if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                        {
                                            hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                        }

                                        _blockStorage.Lock(hash);
                                        lockedHashes.Add(hash);

                                        bool result = _blockStorage.TrySet(hash, bufferMemoryOwner.Memory.Span);

                                        if (!result)
                                        {
                                            throw new ImportFailed("Failed to save Block.");
                                        }

                                        targetHashes.Add(hash);
                                        targetMemoryOwners.Add(bufferMemoryOwner);

                                        if (targetMemoryOwners.Count >= 128)
                                        {
                                            break;
                                        }
                                    }

                                    var parityHashes = await this.ParityEncode(targetMemoryOwners.Select(n => n.Memory), hashAlgorithmType, correctionAlgorithmType, token);
                                    lockedHashes.UnionWith(parityHashes);

                                    merkleTreeSectionList.Add(new MerkleTreeSection(correctionAlgorithmType, sumLength, CollectionHelper.Unite(targetHashes, parityHashes).ToArray()));
                                }
                                finally
                                {
                                    foreach (var memoryOwner in targetMemoryOwners)
                                    {
                                        memoryOwner.Dispose();
                                    }
                                }

                                if (stream.Position == stream.Length)
                                {
                                    break;
                                }
                            }

                            depth++;

                            stream.Dispose();

                            stream = new RecyclableMemoryStream(_bufferPool);
                            RocketPackHelper.MessageToStream(new MerkleTreeNode(merkleTreeSectionList.ToArray()), stream);
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
                finally
                {
                    stream.Dispose();
                }

                lock (_lockObject)
                {
                    if (!_contentMetadataStorage.ContainsMessageContentMetadata(clue))
                    {
                        _contentMetadataStorage.Add(new ContentMetadata(clue, lockedHashes.ToArray(), null));
                    }
                    else
                    {
                        foreach (var hash in lockedHashes)
                        {
                            _blockStorage.Unlock(hash);
                        }
                    }
                }

                return clue;
            }, token);
        }

        public async ValueTask<XeusClue> Import(string path, CancellationToken token = default)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return await Task.Run(async () =>
            {
                // Check
                lock (_lockObject)
                {
                    var info = _contentMetadataStorage.GetFileContentMetadata(path);
                    if (info != null)
                    {
                        return info.Clue;
                    }
                }

                XeusClue? clue = null;
                var lockedHashes = new HashSet<OmniHash>();
                SharedBlocksMetadata? sharedBlocksInfo = null;

                {
                    const int blockLength = 1024 * 1024;
                    const OmniHashAlgorithmType hashAlgorithmType = OmniHashAlgorithmType.Sha2_256;
                    const CorrectionAlgorithmType correctionAlgorithmType = CorrectionAlgorithmType.ReedSolomon8;

                    byte depth = 0;

                    var merkleTreeSectionList = new List<MerkleTreeSection>();

                    // File
                    using (var stream = new UnbufferedFileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, _bufferPool))
                    {
                        if (stream.Length <= blockLength)
                        {
                            OmniHash hash;

                            using (var bufferMemoryOwner = _bufferPool.Rent((int)stream.Length))
                            {
                                stream.Read(bufferMemoryOwner.Memory.Span);

                                if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                {
                                    hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                }
                            }

                            sharedBlocksInfo = new SharedBlocksMetadata(path, (ulong)stream.Length, (uint)stream.Length, new OmniHash[] { hash });
                            clue = new XeusClue(hash, depth);
                        }
                        else
                        {
                            var sharedHashes = new List<OmniHash>();

                            for (; ; )
                            {
                                var targetHashes = new List<OmniHash>();
                                var targetMemoryOwners = new List<IMemoryOwner<byte>>();
                                ulong sumLength = 0;

                                try
                                {
                                    for (int i = 0; stream.Position < stream.Length; i++)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        uint length = (uint)Math.Min(stream.Length - stream.Position, blockLength);
                                        var bufferMemoryOwner = _bufferPool.Rent((int)length);

                                        try
                                        {
                                            stream.Read(bufferMemoryOwner.Memory.Span);

                                            sumLength += length;
                                        }
                                        catch (Exception e)
                                        {
                                            bufferMemoryOwner.Dispose();

                                            throw e;
                                        }

                                        OmniHash hash;

                                        if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                        {
                                            hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                        }

                                        sharedHashes.Add(hash);

                                        targetHashes.Add(hash);
                                        targetMemoryOwners.Add(bufferMemoryOwner);

                                        if (targetMemoryOwners.Count >= 128)
                                        {
                                            break;
                                        }
                                    }

                                    var parityHashes = await this.ParityEncode(targetMemoryOwners.Select(n => n.Memory), hashAlgorithmType, correctionAlgorithmType, token);
                                    lockedHashes.UnionWith(parityHashes);

                                    merkleTreeSectionList.Add(new MerkleTreeSection(correctionAlgorithmType, sumLength, CollectionHelper.Unite(targetHashes, parityHashes).ToArray()));
                                }
                                finally
                                {
                                    foreach (var memoryOwner in targetMemoryOwners)
                                    {
                                        memoryOwner.Dispose();
                                    }
                                }

                                if (stream.Position == stream.Length)
                                {
                                    break;
                                }
                            }

                            sharedBlocksInfo = new SharedBlocksMetadata(path, (ulong)stream.Length, blockLength, sharedHashes.ToArray());

                            depth++;
                        }
                    }

                    while (merkleTreeSectionList.Count > 0)
                    {
                        // Index
                        using (var stream = new RecyclableMemoryStream(_bufferPool))
                        {
                            RocketPackHelper.MessageToStream(new MerkleTreeNode(merkleTreeSectionList.ToArray()), stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            merkleTreeSectionList.Clear();

                            if (stream.Length <= blockLength)
                            {
                                OmniHash hash;

                                using (var bufferMemoryOwner = _bufferPool.Rent((int)stream.Length))
                                {
                                    stream.Read(bufferMemoryOwner.Memory.Span);

                                    if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                    {
                                        hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                    }

                                    _blockStorage.Lock(hash);

                                    bool result = _blockStorage.TrySet(hash, bufferMemoryOwner.Memory.Span);

                                    if (!result)
                                    {
                                        throw new ImportFailed("Failed to save Block.");
                                    }

                                    lockedHashes.Add(hash);
                                }

                                clue = new XeusClue(hash, depth);
                            }
                            else
                            {
                                for (; ; )
                                {
                                    var targetHashes = new List<OmniHash>();
                                    var targetMemoryOwners = new List<IMemoryOwner<byte>>();
                                    ulong sumLength = 0;

                                    try
                                    {
                                        for (int i = 0; stream.Position < stream.Length; i++)
                                        {
                                            token.ThrowIfCancellationRequested();

                                            uint length = (uint)Math.Min(stream.Length - stream.Position, blockLength);
                                            var bufferMemoryOwner = _bufferPool.Rent((int)length);

                                            try
                                            {
                                                stream.Read(bufferMemoryOwner.Memory.Span);

                                                sumLength += length;
                                            }
                                            catch (Exception e)
                                            {
                                                bufferMemoryOwner.Dispose();

                                                throw e;
                                            }

                                            OmniHash hash;

                                            if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                                            {
                                                hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bufferMemoryOwner.Memory.Span));
                                            }

                                            _blockStorage.Lock(hash);

                                            bool result = _blockStorage.TrySet(hash, bufferMemoryOwner.Memory.Span);

                                            if (!result)
                                            {
                                                throw new ImportFailed("Failed to save Block.");
                                            }

                                            lockedHashes.Add(hash);

                                            targetHashes.Add(hash);
                                            targetMemoryOwners.Add(bufferMemoryOwner);

                                            if (targetMemoryOwners.Count >= 128)
                                            {
                                                break;
                                            }
                                        }

                                        var parityHashes = await this.ParityEncode(targetMemoryOwners.Select(n => n.Memory), hashAlgorithmType, correctionAlgorithmType, token);
                                        lockedHashes.UnionWith(parityHashes);

                                        merkleTreeSectionList.Add(new MerkleTreeSection(correctionAlgorithmType, sumLength, CollectionHelper.Unite(targetHashes, parityHashes).ToArray()));
                                    }
                                    finally
                                    {
                                        foreach (var memoryOwner in targetMemoryOwners)
                                        {
                                            memoryOwner.Dispose();
                                        }
                                    }

                                    if (stream.Position == stream.Length)
                                    {
                                        break;
                                    }
                                }

                                depth++;
                            }
                        }
                    }
                }

                if (clue == null)
                {
                    throw new ImportFailed("clue is null");
                }

                lock (_lockObject)
                {
                    if (!_contentMetadataStorage.ContainsFileContentMetadata(path))
                    {
                        _contentMetadataStorage.Add(new ContentMetadata(clue, lockedHashes.ToArray(), sharedBlocksInfo));

                        foreach (var hash in lockedHashes)
                        {
                            _blockStorage.Lock(hash);
                        }
                    }
                }

                return clue;
            }, token);
        }

        private async ValueTask<OmniHash[]> ParityEncode(IEnumerable<Memory<byte>> buffers, OmniHashAlgorithmType hashAlgorithmType, CorrectionAlgorithmType correctionAlgorithmType, CancellationToken token = default)
        {
            return await Task.Run(async () =>
            {
                if (correctionAlgorithmType == CorrectionAlgorithmType.ReedSolomon8)
                {
                    if (buffers.Count() > 128)
                    {
                        throw new ArgumentOutOfRangeException(nameof(buffers));
                    }

                    var createdMemoryOwners = new List<IMemoryOwner<byte>>();

                    try
                    {
                        var targetBuffers = new ReadOnlyMemory<byte>[buffers.Count()];
                        var parityMemoryOwners = new IMemoryOwner<byte>[buffers.Count()];

                        int blockLength = buffers.Max(n => n.Length);

                        // Normalize
                        {
                            int index = 0;

                            foreach (var buffer in buffers)
                            {
                                token.ThrowIfCancellationRequested();

                                if (buffer.Length < blockLength)
                                {
                                    var tempMemoryOwner = _bufferPool.Rent(blockLength);

                                    BytesOperations.Copy(buffer.Span, tempMemoryOwner.Memory.Span, buffer.Length);
                                    BytesOperations.Zero(tempMemoryOwner.Memory.Span.Slice(buffer.Length));

                                    createdMemoryOwners.Add(tempMemoryOwner);

                                    targetBuffers[index] = tempMemoryOwner.Memory;
                                }
                                else
                                {
                                    targetBuffers[index] = buffer;
                                }

                                index++;
                            }
                        }

                        for (int i = 0; i < parityMemoryOwners.Length; i++)
                        {
                            parityMemoryOwners[i] = _bufferPool.Rent(blockLength);
                        }

                        var indexes = new int[parityMemoryOwners.Length];

                        for (int i = 0; i < parityMemoryOwners.Length; i++)
                        {
                            indexes[i] = targetBuffers.Length + i;
                        }

                        var reedSolomon = new ReedSolomon8(targetBuffers.Length, targetBuffers.Length + parityMemoryOwners.Length, _bufferPool);
                        await reedSolomon.Encode(targetBuffers, indexes, parityMemoryOwners.Select(n => n.Memory).ToArray(), blockLength, _threadCount, token);

                        token.ThrowIfCancellationRequested();

                        var parityHashes = new List<OmniHash>();

                        for (int i = 0; i < parityMemoryOwners.Length; i++)
                        {
                            OmniHash hash;

                            if (hashAlgorithmType == OmniHashAlgorithmType.Sha2_256)
                            {
                                hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(parityMemoryOwners[i].Memory.Span));
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }

                            _blockStorage.Lock(hash);

                            bool result = _blockStorage.TrySet(hash, parityMemoryOwners[i].Memory.Span);

                            if (!result)
                            {
                                throw new ImportFailed("Failed to save Block.");
                            }

                            parityHashes.Add(hash);
                        }

                        return parityHashes.ToArray();
                    }
                    finally
                    {
                        foreach (var memoryOwner in createdMemoryOwners)
                        {
                            memoryOwner.Dispose();
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            });
        }

        #region Message

        private void CheckMessages()
        {
            lock (_lockObject)
            {
                foreach (var contentInfo in _contentMetadataStorage.GetMessageContentMetadatas())
                {
                    if (contentInfo.LockedHashes.All(n => this.Contains(n)))
                    {
                        continue;
                    }

                    this.RemoveMessage(contentInfo.Clue);
                }
            }
        }

        public void RemoveMessage(XeusClue clue)
        {
            lock (_lockObject)
            {
                var contentInfo = _contentMetadataStorage.GetMessageContentMetadata(clue);
                if (contentInfo == null)
                {
                    return;
                }

                _contentMetadataStorage.RemoveMessageContentMetadata(clue);

                foreach (var hash in contentInfo.LockedHashes)
                {
                    _blockStorage.Unlock(hash);
                }

                if (contentInfo.SharedBlocksMetadata != null)
                {
                    // Event
                    _removedBlockEventQueue.Enqueue(contentInfo.SharedBlocksMetadata.Hashes.Where(n => !this.Contains(n)).ToArray());
                }
            }
        }

        #endregion

        #region Content

        private void CheckContents()
        {
            lock (_lockObject)
            {
                foreach (var contentInfo in _contentMetadataStorage.GetFileContentMetadatas())
                {
                    if (contentInfo.LockedHashes.All(n => this.Contains(n)))
                    {
                        continue;
                    }

                    if (contentInfo.SharedBlocksMetadata != null)
                    {
                        this.RemoveContent(contentInfo.SharedBlocksMetadata.Path);
                    }
                }
            }
        }

        public void RemoveContent(string path)
        {
            lock (_lockObject)
            {
                var contentInfo = _contentMetadataStorage.GetFileContentMetadata(path);
                if (contentInfo == null)
                {
                    return;
                }

                _contentMetadataStorage.RemoveFileContentMetadata(path);

                foreach (var hash in contentInfo.LockedHashes)
                {
                    _blockStorage.Unlock(hash);
                }

                if (contentInfo.SharedBlocksMetadata != null)
                {
                    // Event
                    _removedBlockEventQueue.Enqueue(contentInfo.SharedBlocksMetadata.Hashes.Where(n => !this.Contains(n)).ToArray());
                }
            }
        }

        public IEnumerable<OmniHash> GetContentHashes(string path)
        {
            lock (_lockObject)
            {
                var contentInfo = _contentMetadataStorage.GetFileContentMetadata(path);
                if (contentInfo == null)
                {
                    return Enumerable.Empty<OmniHash>();
                }

                return contentInfo.LockedHashes.ToArray();
            }
        }

        #endregion

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

                await _blockStorage.LoadAsync();

                if (_settings.TryGetContent<ContentStorageConfig>("Config", out var config))
                {
                    foreach (var contentInfo in config.ContentMetadatas)
                    {
                        _contentMetadataStorage.Add(contentInfo);

                        foreach (var hash in contentInfo.LockedHashes)
                        {
                            _blockStorage.Lock(hash);
                        }
                    }
                }

                _checkEventScheduler.ChangeInterval(new TimeSpan(0, 10, 0));
                await _checkEventScheduler.RestartAsync();
            }
        }

        public async ValueTask SaveAsync()
        {
            using (await _settingsAsyncLock.LockAsync())
            {
                await _blockStorage.SaveAsync();

                var config = new ContentStorageConfig(0, _contentMetadataStorage.ToArray());
                _settings.SetContent("Config", config);
            }
        }

        #endregion

        public OmniHash[] ToArray()
        {
            lock (_lockObject)
            {
                var hashSet = new HashSet<OmniHash>();
                hashSet.UnionWith(_blockStorage.ToArray());
                hashSet.UnionWith(_contentMetadataStorage.GetHashes());

                return hashSet.ToArray();
            }
        }

        #region IEnumerable<OmniHash>

        public IEnumerator<OmniHash> GetEnumerator()
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
            if (disposing)
            {
                _settings.Dispose();

                _removedBlockEventQueue.Dispose();

                _blockStorage.Dispose();
                _checkEventScheduler.Dispose();
            }
        }
    }

    internal sealed class BlockNotFoundException : Exception
    {
        public BlockNotFoundException() { }
        public BlockNotFoundException(string message) : base(message) { }
        public BlockNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    internal sealed class ParityDecodeFailed : Exception
    {
        public ParityDecodeFailed() { }
        public ParityDecodeFailed(string message) : base(message) { }
        public ParityDecodeFailed(string message, Exception innerException) : base(message, innerException) { }
    }

    internal sealed class ImportFailed : Exception
    {
        public ImportFailed() { }
        public ImportFailed(string message) : base(message) { }
        public ImportFailed(string message, Exception innerException) : base(message, innerException) { }
    }
}
