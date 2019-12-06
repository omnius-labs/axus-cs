using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;

namespace Omnius.Xeus.Engine.Storages
{
    public sealed class FileStorage : DisposableBase, IFileStorage
    {
        private Dictionary<string, PublishFileStatus> _publishFileStatusMap = new Dictionary<string, PublishFileStatus>();
        private Dictionary<(OmniHash, string), WantFileStatus> _wantFileStatusMap = new Dictionary<(OmniHash, string), WantFileStatus>();

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

        public async ValueTask<OmniHash?> AddPublishFile(string filePath, CancellationToken cancellationToken = default)
        {
            if (_publishFileStatusMap.ContainsKey(filePath)) return null;

            var status = new PublishFileStatus(filePath);

            _publishFileStatusMap.Add(filePath, status);
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

        public void AddWantFile(OmniHash rootHash, string filePath)
        {
            var key = (rootHash, filePath);
            if (_wantFileStatusMap.ContainsKey(key)) return;

            var status = new WantFileStatus(rootHash, filePath);
            status.CurrentDepth = 0;

            _wantFileStatusMap.Add(key, status);
        }

        public void RemoveWantFile(OmniHash rootHash, string filePath)
        {
            var key = (rootHash, filePath);
            _wantFileStatusMap.Remove(key);
        }

        public IEnumerable<WantFileReport> GetWantFileReports()
        {
            foreach (var status in _wantFileStatusMap.Values)
            {
                yield return new WantFileReport();
            }
        }

        #endregion

        protected override async ValueTask OnDisposeAsync()
        {

        }

        protected override void OnDispose(bool disposing)
        {

        }

        class PublishFileStatus
        {
            public PublishFileStatus(string filePath)
            {
                this.FilePath = filePath;
            }

            public string FilePath { get; }

            public OmniHash RootHash { get; set; }
            public int CurrentDepth { get; set; }
            public List<OmniHash> WantBlocks { get; } = new List<OmniHash>();
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
