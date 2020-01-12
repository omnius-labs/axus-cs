using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Internal;

namespace Omnius.Xeus.Service
{
    public sealed partial class FileStorage
    {
        private sealed class WantFileStorage
        {
            private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

            private readonly string _configPath;
            private readonly IBufferPool<byte> _bufferPool;

            private readonly Dictionary<OmniHash, WantFileStatus> _wantFileStatusMap = new Dictionary<OmniHash, WantFileStatus>();

            private readonly AsyncLock _asyncLock = new AsyncLock();

            public WantFileStorage(string configPath, IBufferPool<byte> bufferPool)
            {
                _configPath = configPath;
                _bufferPool = bufferPool;
            }

            // TODO: WantBlocksの更新を別スレッドで行わなければならない。

            private string OmniHashToString(OmniHash hash)
            {
                using var hub = new Hub(_bufferPool);
                hash.Export(hub.Writer, _bufferPool);

                var value = OmniBase.ToBase58BtcString(hub.Reader.GetSequence());

                return Path.Combine(_configPath, value);
            }

            public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    if (!_wantFileStatusMap.ContainsKey(rootHash))
                    {
                        return null;
                    }

                    var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToString(rootHash)), this.OmniHashToString(targetHash));

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }

                    using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bufferPool))
                    {
                        var memoryOwner = _bufferPool.RentMemory((int)fileStream.Length);
                        await fileStream.ReadAsync(memoryOwner.Memory);

                        return memoryOwner;
                    }
                }
            }

            public async ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    if (!_wantFileStatusMap.TryGetValue(rootHash, out var status)
                        || !status.WantBlocks.Contains(targetHash))
                    {
                        return;
                    }

                    var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToString(rootHash)), this.OmniHashToString(targetHash));

                    if (File.Exists(filePath))
                    {
                        return;
                    }

                    using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bufferPool))
                    {
                        await fileStream.WriteAsync(memory);
                    }

                    status.WantBlocks.Remove(targetHash);
                }
            }

            public async ValueTask AddWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    if (_wantFileStatusMap.ContainsKey(rootHash))
                    {
                        return;
                    }

                    var status = new WantFileStatus(rootHash, filePath);
                    status.CurrentDepth = 0;
                    status.WantBlocks.Add(rootHash);

                    _wantFileStatusMap.Add(rootHash, status);
                }
            }

            public async ValueTask RemoveWantFile(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    _wantFileStatusMap.Remove(rootHash);
                }
            }

            public async IAsyncEnumerable<WantFileReport> GetWantFileReportsAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    foreach (var status in _wantFileStatusMap.Values)
                    {
                        yield return new WantFileReport(status.RootHash, status.FilePath);
                    }
                }
            }

            class WantFileStatus
            {
                public WantFileStatus(OmniHash rootHash, string filePath)
                {
                    this.FilePath = filePath;
                    this.RootHash = rootHash;
                }

                public string FilePath { get; }
                public OmniHash RootHash { get; }

                public int CurrentDepth { get; set; }
                public HashSet<OmniHash> WantBlocks { get; } = new HashSet<OmniHash>();
            }
        }
    }
}
