using System;
using System.Collections.Generic;
using System.IO;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Io;
using System.Linq;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public partial class BlocksManager
        {
            sealed class ProtectionManager
            {
                private Dictionary<Hash, int> _lockedHashes = new Dictionary<Hash, int>();

                public ProtectionManager()
                {

                }

                public IEnumerable<Hash> GetHashes()
                {
                    return _lockedHashes.Keys.ToArray();
                }

                public bool Contains(Hash hash)
                {
                    return _lockedHashes.ContainsKey(hash);
                }

                public void Add(Hash hash)
                {
                    int count;
                    _lockedHashes.TryGetValue(hash, out count);

                    count++;

                    _lockedHashes[hash] = count;
                }

                public void Remove(Hash hash)
                {
                    int count;
                    if (!_lockedHashes.TryGetValue(hash, out count)) throw new KeyNotFoundException();

                    count--;

                    if (count == 0)
                    {
                        _lockedHashes.Remove(hash);
                    }
                    else
                    {
                        _lockedHashes[hash] = count;
                    }
                }
            }
        }
    }
}
