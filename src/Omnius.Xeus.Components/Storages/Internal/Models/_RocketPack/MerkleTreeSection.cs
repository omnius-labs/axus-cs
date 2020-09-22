using System.Collections.Generic;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Components.Storages.Internal.Models
{
    partial class MerkleTreeSection
    {
        private Dictionary<OmniHash, long> _indexMap = null;
        private readonly object _lockObject = new object();

        private Dictionary<OmniHash, long> GetIndexMap()
        {
            if (_indexMap == null)
            {
                lock (_lockObject)
                {
                    if (_indexMap == null)
                    {
                        _indexMap = new Dictionary<OmniHash, long>();

                        for (int i = 0; i < this.Hashes.Count; i++)
                        {
                            _indexMap[this.Hashes[i]] = i;
                        }
                    }
                }
            }

            return _indexMap;
        }

        public bool Contains(OmniHash targetHash)
        {
            return this.GetIndexMap().ContainsKey(targetHash);
        }

        public bool TryGetIndex(OmniHash targetHash, out long index)
        {
            return this.GetIndexMap().TryGetValue(targetHash, out index);
        }
    }
}
