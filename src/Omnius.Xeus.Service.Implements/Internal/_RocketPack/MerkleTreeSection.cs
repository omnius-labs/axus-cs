using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Internal
{
    partial class MerkleTreeSection
    {
        private Dictionary<OmniHash, long> _indexMap = null;
        private readonly object _lockObject = new object();

        public bool TryGetIndex(OmniHash targetHash, out long index)
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

            return _indexMap.TryGetValue(targetHash, out index);
        }
    }
}
