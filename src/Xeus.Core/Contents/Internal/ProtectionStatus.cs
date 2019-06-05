using System;
using System.Collections.Generic;
using System.IO;
using Omnix.Base;
using Omnix.Io;
using System.Linq;
using Omnix.Cryptography;

namespace Xeus.Core.Contents.Internal
{
    internal sealed class ProtectionStatus
    {
        private Dictionary<OmniHash, int> _lockedHashMap = new Dictionary<OmniHash, int>();

        public ProtectionStatus()
        {

        }

        public IEnumerable<OmniHash> GetProtectedHashes()
        {
            return _lockedHashMap.Keys.ToArray();
        }

        public bool Contains(OmniHash hash)
        {
            return _lockedHashMap.ContainsKey(hash);
        }

        public void Add(OmniHash hash)
        {
            int count;
            _lockedHashMap.TryGetValue(hash, out count);

            count++;

            _lockedHashMap[hash] = count;
        }

        public void Remove(OmniHash hash)
        {
            int count;
            if (!_lockedHashMap.TryGetValue(hash, out count))
            {
                throw new KeyNotFoundException();
            }

            count--;

            if (count == 0)
            {
                _lockedHashMap.Remove(hash);
            }
            else
            {
                _lockedHashMap[hash] = count;
            }
        }
    }
}
