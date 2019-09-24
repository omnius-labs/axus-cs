using System.Collections.Generic;
using System.Linq;
using Omnix.Algorithms.Cryptography;

namespace Xeus.Core.Storage.Internal
{
    internal sealed class ProtectionStatus
    {
        private readonly Dictionary<OmniHash, int> _lockedHashMap = new Dictionary<OmniHash, int>();

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
            _lockedHashMap.TryGetValue(hash, out int count);

            count++;

            _lockedHashMap[hash] = count;
        }

        public void Remove(OmniHash hash)
        {
            if (!_lockedHashMap.TryGetValue(hash, out int count))
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
