using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Amoeba.Messages;
using Omnix.Base;
using Omnix.Collections;
using Omnix.Cryptography;

namespace Xeus.Engine.Contents.Download.Internal
{
    internal sealed class ExistManager
    {
        private Dictionary<MerkleTreeSection, MerkleTreeSectionManager> _map;

        private readonly object _lockObject = new object();

        public ExistManager()
        {
            // ŽQÆ‚Ì”äŠr‚ðs‚¤Map‚ðì¬‚·‚éB
            int getHashCode(MerkleTreeSection value) => RuntimeHelpers.GetHashCode(value);
            bool predicate(MerkleTreeSection x, MerkleTreeSection y) => object.ReferenceEquals(x, y);
            _map = new Dictionary<MerkleTreeSection, MerkleTreeSectionManager>(new GenericEqualityComparer<MerkleTreeSection>(predicate, getHashCode));
        }

        public void Add(MerkleTreeSection merkleTreeSection)
        {
            lock (_lockObject)
            {
                _map[merkleTreeSection] = new MerkleTreeSectionManager(merkleTreeSection);
            }
        }

        public void Remove(MerkleTreeSection merkleTreeSection)
        {
            lock (_lockObject)
            {
                _map.Remove(merkleTreeSection);
            }
        }

        public void Set(IEnumerable<OmniHash> hashes, bool state)
        {
            lock (_lockObject)
            {
                foreach (var merkleTreeSectionManager in _map.Values)
                {
                    foreach (var hash in hashes)
                    {
                        merkleTreeSectionManager.Set(hash, state);
                    }
                }
            }
        }

        public IEnumerable<OmniHash> GetOmniHashes(MerkleTreeSection merkleTreeSection, bool state)
        {
            lock (_lockObject)
            {
                if (_map.TryGetValue(merkleTreeSection, out var merkleTreeSectionManager))
                {
                    return merkleTreeSectionManager.GetHashes(state);
                }
                else
                {
                    if (state)
                    {
                        return Enumerable.Empty<OmniHash>();
                    }
                    else
                    {
                        return merkleTreeSection.Hashes;
                    }
                }
            }
        }

        public int GetCount(MerkleTreeSection merkleTreeSection, bool state)
        {
            lock (_lockObject)
            {
                if (_map.TryGetValue(merkleTreeSection, out var merkleTreeSectionManager))
                {
                    return merkleTreeSectionManager.GetCount(state);
                }
                else
                {
                    if (state)
                    {
                        return 0;
                    }
                    else
                    {
                        return merkleTreeSection.Hashes.Count;
                    }
                }
            }
        }

        private class MerkleTreeSectionManager
        {
            private BloomFilter<OmniHash> _bloomFilter;
            private Dictionary<OmniHash, State> _dic;

            private class State
            {
                public bool IsEnabled { get; set; }
                public int Count { get; set; }
            }

            public MerkleTreeSectionManager(MerkleTreeSection merkleTreeSection)
            {
                _bloomFilter = new BloomFilter<OmniHash>(256, 0.001, (n) => n.GetHashCode());

                _dic = new Dictionary<OmniHash, State>();

                foreach (var key in merkleTreeSection.Hashes)
                {
                    _bloomFilter.Add(key);

                    State info;

                    if (!_dic.TryGetValue(key, out info))
                    {
                        info = new State();
                        info.IsEnabled = false;
                        info.Count = 0;

                        _dic.Add(key, info);
                    }

                    info.Count++;
                }
            }

            public void Set(OmniHash key, bool state)
            {
                if (!_bloomFilter.Contains(key)) return;

                State info;
                if (!_dic.TryGetValue(key, out info)) return;

                info.IsEnabled = state;
            }

            public IEnumerable<OmniHash> GetHashes(bool state)
            {
                var list = new List<OmniHash>();

                foreach (var (hash, info) in _dic)
                {
                    if (info.IsEnabled == state)
                    {
                        for (int i = 0; i < info.Count; i++)
                        {
                            list.Add(hash);
                        }
                    }
                }

                return list;
            }

            public int GetCount(bool state)
            {
                int sum = 0;

                foreach (var info in _dic.Values)
                {
                    if (info.IsEnabled == state)
                    {
                        sum += info.Count;
                    }
                }

                return sum;
            }
        }
    }
}
