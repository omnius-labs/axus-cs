using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Xeus.Service.Internal;

namespace Omnius.Xeus.Service
{
    public sealed class FileStorage : DisposableBase, IFileStorage
    {
        private readonly Dictionary<string, PublishFileStatus> _publishFileStatusMap = new Dictionary<string, PublishFileStatus>();
        private readonly Dictionary<string, WantFileStatus> _wantFileStatusMap = new Dictionary<string, WantFileStatus>();

        public ulong TotalUsingBytes { get; }

        public ValueTask CheckConsistency(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public uint GetLength(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner)
        {
            throw new NotImplementedException();
        }

        public bool TryWrite(OmniHash hash, ReadOnlySpan<byte> value)
        {
            throw new NotImplementedException();
        }

        #region PublishFile

        public async ValueTask<OmniHash> AddPublishFile(string filePath, CancellationToken cancellationToken = default)
        {
            {
                if (_publishFileStatusMap.TryGetValue(filePath, out var status))
                {
                    return status.RootHash;
                }
            }

            {
                const int MaxBlockLength = 1 * 1024 * 1024;

                using(var fileStream = new FileStream(filePath, FileMode.Open))
                {

                }

                var status = new PublishFileStatus(filePath, default, null);

                _publishFileStatusMap.Add(filePath, status);

                throw new NotSupportedException();
            }
        }

        public void RemovePublishFile(string path)
        {
            _publishFileStatusMap.Remove(path);
        }

        public IEnumerable<PublishFileReport> GetPublishFileReports()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region WantFile

        public void AddWantFile( string filePath, OmniHash rootHash)
        {
            if (_wantFileStatusMap.ContainsKey(filePath)) return;

            var status = new WantFileStatus(rootHash, filePath);
            status.CurrentDepth = 0;

            _wantFileStatusMap.Add(filePath, status);
        }

        public void RemoveWantFile(string filePath, OmniHash rootHash)
        {
            _wantFileStatusMap.Remove(filePath);
        }

        public IEnumerable<WantFileReport> GetWantFileReports()
        {
            foreach (var status in _wantFileStatusMap.Values)
            {
                yield return new WantFileReport();
            }
        }

        #endregion

        protected override void OnDispose(bool disposing)
        {

        }

        class PublishFileStatus
        {
            private Dictionary<OmniHash, long> _indexMap = new Dictionary<OmniHash, long>();

            public PublishFileStatus(string filePath, OmniHash rootHash, MerkleTreeSection[] merkleTreeSections)
            {
                this.FilePath = filePath;
                this.RootHash = rootHash;
                this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
            }

            public string FilePath { get; }
            public OmniHash RootHash { get; }
            public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }

            public bool TryGetBlockIndexForFile(OmniHash hash, out long index)
            {
                return _indexMap.TryGetValue(hash, out index);
            }
        }

        class WantFileStatus
        {
            public WantFileStatus(OmniHash rootHash, string filePath)
            {
                this.RootHash = rootHash;
                this.FilePath = filePath;
            }

            public OmniHash RootHash { get; }
            public string FilePath { get; }

            public int CurrentDepth { get; set; }
            public List<OmniHash> WantBlocks { get; } = new List<OmniHash>();
        }
    }
}
