using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Internal;

namespace Omnius.Xeus.Service
{
    public sealed partial class FileStorage
    {
        private readonly Dictionary<OmniHash, WantFileStatus> _wantFileStatusMap = new Dictionary<OmniHash, WantFileStatus>();

        public void AddWantFile(OmniHash rootHash, string filePath)
        {
            if (_wantFileStatusMap.ContainsKey(rootHash)) return;

            var status = new WantFileStatus(rootHash, filePath);
            status.CurrentDepth = 0;

            _wantFileStatusMap.Add(rootHash, status);
        }

        public void RemoveWantFile(OmniHash rootHash, string filePath)
        {
            _wantFileStatusMap.Remove(rootHash);
        }

        public IEnumerable<WantFileReport> GetWantFileReports()
        {
            foreach (var status in _wantFileStatusMap.Values)
            {
                yield return new WantFileReport();
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
            public List<OmniHash> WantBlocks { get; } = new List<OmniHash>();
        }
    }
}
