using System;
using System.Collections.Generic;
using System.Linq;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Collections;
using Omnius.Security;

namespace Amoeba.Service
{
    partial class NetworkManager
    {
        public sealed class MetadataManager
        {
            // Type, AuthorSignature
            private Dictionary<string, Dictionary<Signature, BroadcastMetadata>> _broadcastMetadatas = new Dictionary<string, Dictionary<Signature, BroadcastMetadata>>();
            // Type, Signature, AuthorSignature
            private Dictionary<string, Dictionary<Signature, Dictionary<Signature, HashSet<UnicastMetadata>>>> _unicastMetadatas = new Dictionary<string, Dictionary<Signature, Dictionary<Signature, HashSet<UnicastMetadata>>>>();
            // Type, Tag, AuthorSignature
            private Dictionary<string, Dictionary<Tag, Dictionary<Signature, HashSet<MulticastMetadata>>>> _multicastMetadatas = new Dictionary<string, Dictionary<Tag, Dictionary<Signature, HashSet<MulticastMetadata>>>>();

            // UpdateTime
            private Dictionary<string, DateTime> _broadcastTypes = new Dictionary<string, DateTime>();
            private Dictionary<string, DateTime> _unicastTypes = new Dictionary<string, DateTime>();
            private Dictionary<string, DateTime> _multicastTypes = new Dictionary<string, DateTime>();

            // Alive
            private VolatileHashSet<Tag> _aliveTags = new VolatileHashSet<Tag>(new TimeSpan(0, 30, 0));

            private readonly object _lockObject = new object();

            public MetadataManager()
            {

            }

            public GetSignaturesEventHandler GetLockSignaturesEvent { get; set; }

            private IEnumerable<Signature> OnGetLockSignaturesEvent()
            {
                return this.GetLockSignaturesEvent?.Invoke(this) ?? new Signature[0];
            }

            public void Refresh()
            {
                lock (_lockObject)
                {
                    var lockSignatures = new HashSet<Signature>(this.OnGetLockSignaturesEvent());
                    var lockTags = new HashSet<Tag>(_aliveTags);

                    _aliveTags.Update();

                    // Broadcast
                    {
                        {
                            var removeTypes = new HashSet<string>(_broadcastTypes.OrderBy(n => n.Value).Select(n => n.Key).Take(_broadcastTypes.Count - 32));

                            foreach (string type in removeTypes)
                            {
                                _broadcastTypes.Remove(type);
                            }

                            foreach (string type in _broadcastMetadatas.Keys.ToArray())
                            {
                                if (!removeTypes.Contains(type)) continue;
                                _broadcastMetadatas.Remove(type);
                            }
                        }

                        foreach (var dic in _broadcastMetadatas.Values)
                        {
                            var keys = dic.Keys.Where(n => !lockSignatures.Contains(n)).ToList();

                            foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                            {
                                dic.Remove(key);
                            }
                        }
                    }

                    // Unicast
                    {
                        {
                            var removeTypes = new HashSet<string>(_unicastTypes.OrderBy(n => n.Value).Select(n => n.Key).Take(_unicastTypes.Count - 32));

                            foreach (string type in removeTypes)
                            {
                                _unicastTypes.Remove(type);
                            }

                            foreach (string type in _unicastMetadatas.Keys.ToArray())
                            {
                                if (!removeTypes.Contains(type)) continue;
                                _unicastMetadatas.Remove(type);
                            }
                        }

                        foreach (var dic in _unicastMetadatas.Values)
                        {
                            var keys = dic.Keys.Where(n => !lockSignatures.Contains(n)).ToList();

                            foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                            {
                                dic.Remove(key);
                            }
                        }

                        foreach (var dic in _unicastMetadatas.Values.SelectMany(n => n.Values))
                        {
                            var keys = dic.Keys.Where(n => !lockSignatures.Contains(n)).ToList();

                            foreach (var key in keys.Randomize().Take(keys.Count - 32))
                            {
                                dic.Remove(key);
                            }
                        }

                        foreach (var hashset in _unicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
                        {
                            if (hashset.Count <= 32) continue;

                            var list = hashset.ToList();
                            list.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

                            foreach (var value in list.Take(list.Count - 32))
                            {
                                hashset.Remove(value);
                            }
                        }
                    }

                    // Multicast
                    {
                        {
                            var removeTypes = new HashSet<string>(_multicastTypes.OrderBy(n => n.Value).Select(n => n.Key).Take(_multicastTypes.Count - 32));

                            foreach (string type in removeTypes)
                            {
                                _multicastTypes.Remove(type);
                            }

                            foreach (string type in _multicastMetadatas.Keys.ToArray())
                            {
                                if (!removeTypes.Contains(type)) continue;
                                _multicastMetadatas.Remove(type);
                            }
                        }

                        foreach (var dic in _multicastMetadatas.Values)
                        {
                            var keys = dic.Keys.Where(n => !lockTags.Contains(n)).ToList();

                            foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                            {
                                dic.Remove(key);
                            }
                        }

                        foreach (var dic in _multicastMetadatas.Values.SelectMany(n => n.Values))
                        {
                            var keys = dic.Keys.Where(n => !lockSignatures.Contains(n)).ToList();

                            foreach (var key in keys.Randomize().Take(keys.Count - 32))
                            {
                                dic.Remove(key);
                            }
                        }

                        foreach (var hashset in _multicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
                        {
                            if (hashset.Count <= 32) continue;

                            var list = hashset.ToList();
                            list.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

                            foreach (var value in list.Take(list.Count - 32))
                            {
                                hashset.Remove(value);
                            }
                        }
                    }
                }
            }

            public int Count
            {
                get
                {
                    lock (_lockObject)
                    {
                        int count = 0;

                        count += _broadcastMetadatas.Values.Sum(n => n.Count);
                        count += _unicastMetadatas.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));
                        count += _multicastMetadatas.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));

                        return count;
                    }
                }
            }

            public IEnumerable<Signature> GetBroadcastSignatures()
            {
                lock (_lockObject)
                {
                    var hashset = new HashSet<Signature>();

                    hashset.UnionWith(_broadcastMetadatas.Values.SelectMany(n => n.Keys));

                    return hashset;
                }
            }

            public IEnumerable<Signature> GetUnicastSignatures()
            {
                lock (_lockObject)
                {
                    var hashset = new HashSet<Signature>();

                    hashset.UnionWith(_unicastMetadatas.Values.SelectMany(n => n.Keys));

                    return hashset;
                }
            }

            public IEnumerable<Tag> GetMulticastTags()
            {
                lock (_lockObject)
                {
                    var hashset = new HashSet<Tag>();

                    hashset.UnionWith(_multicastMetadatas.Values.SelectMany(n => n.Keys));

                    return hashset;
                }
            }

            public IEnumerable<BroadcastMetadata> GetBroadcastMetadatas()
            {
                lock (_lockObject)
                {
                    return _broadcastMetadatas.Values.SelectMany(n => n.Values).ToArray();
                }
            }

            public IEnumerable<BroadcastMetadata> GetBroadcastMetadatas(Signature signature)
            {
                lock (_lockObject)
                {
                    var list = new List<BroadcastMetadata>();

                    foreach (var dic in _broadcastMetadatas.Values)
                    {
                        if (dic.TryGetValue(signature, out var broadcastMetadata))
                        {
                            list.Add(broadcastMetadata);
                        }
                    }

                    return list;
                }
            }

            public BroadcastMetadata GetBroadcastMetadata(Signature signature, string type)
            {
                lock (_lockObject)
                {
                    _broadcastTypes[type] = DateTime.UtcNow;

                    if (_broadcastMetadatas.TryGetValue(type, out var dic))
                    {
                        if (dic.TryGetValue(signature, out var broadcastMetadata))
                        {
                            return broadcastMetadata;
                        }
                    }

                    return null;
                }
            }

            public IEnumerable<UnicastMetadata> GetUnicastMetadatas()
            {
                lock (_lockObject)
                {
                    return _unicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.Extract())).ToArray();
                }
            }

            public IEnumerable<UnicastMetadata> GetUnicastMetadatas(Signature signature)
            {
                lock (_lockObject)
                {
                    var list = new List<UnicastMetadata>();

                    foreach (var dic in _unicastMetadatas.Values)
                    {
                        if (dic.TryGetValue(signature, out var dic2))
                        {
                            list.AddRange(dic2.Values.Extract());
                        }
                    }

                    return list;
                }
            }

            public IEnumerable<UnicastMetadata> GetUnicastMetadatas(Signature signature, string type)
            {
                lock (_lockObject)
                {
                    _unicastTypes[type] = DateTime.UtcNow;

                    var list = new List<UnicastMetadata>();

                    if (_unicastMetadatas.TryGetValue(type, out var dic))
                    {
                        if (dic.TryGetValue(signature, out var dic2))
                        {
                            list.AddRange(dic2.Values.Extract());
                        }
                    }

                    return list;
                }
            }

            public IEnumerable<MulticastMetadata> GetMulticastMetadatas()
            {
                lock (_lockObject)
                {
                    return _multicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.Extract())).ToArray();
                }
            }

            public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Tag tag)
            {
                lock (_lockObject)
                {
                    var list = new List<MulticastMetadata>();

                    foreach (var dic in _multicastMetadatas.Values)
                    {
                        if (dic.TryGetValue(tag, out var dic2))
                        {
                            list.AddRange(dic2.Values.Extract());
                        }
                    }

                    return list;
                }
            }

            public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Tag tag, string type)
            {
                lock (_lockObject)
                {
                    _aliveTags.Add(tag);
                    _multicastTypes[type] = DateTime.UtcNow;

                    var list = new List<MulticastMetadata>();

                    if (_multicastMetadatas.TryGetValue(type, out var dic))
                    {
                        if (dic.TryGetValue(tag, out var dic2))
                        {
                            list.AddRange(dic2.Values.Extract());
                        }
                    }

                    return list;
                }
            }

            public bool SetMetadata(BroadcastMetadata broadcastMetadata)
            {
                lock (_lockObject)
                {
                    var now = DateTime.UtcNow;

                    if (broadcastMetadata == null
                        || broadcastMetadata.Type == null
                        || (broadcastMetadata.CreationTime - now).TotalMinutes > 30
                        || broadcastMetadata.Certificate == null) return false;

                    if (!_broadcastMetadatas.TryGetValue(broadcastMetadata.Type, out var dic))
                    {
                        dic = new Dictionary<Signature, BroadcastMetadata>();
                        _broadcastMetadatas[broadcastMetadata.Type] = dic;
                    }

                    var signature = broadcastMetadata.Certificate.GetSignature();

                    if (!dic.TryGetValue(signature, out var tempMetadata)
                        || broadcastMetadata.CreationTime > tempMetadata.CreationTime)
                    {
                        if (!broadcastMetadata.VerifyCertificate()) return false;

                        dic[signature] = broadcastMetadata;
                    }

                    return true;
                }
            }

            public bool SetMetadata(UnicastMetadata unicastMetadata)
            {
                lock (_lockObject)
                {
                    var now = DateTime.UtcNow;

                    if (unicastMetadata == null
                        || unicastMetadata.Type == null
                        || unicastMetadata.Signature == null
                            || unicastMetadata.Signature.Id == null || unicastMetadata.Signature.Id.Length == 0
                            || string.IsNullOrWhiteSpace(unicastMetadata.Signature.Name)
                        || (unicastMetadata.CreationTime - now).TotalMinutes > 30
                        || unicastMetadata.Certificate == null) return false;

                    if (!_unicastMetadatas.TryGetValue(unicastMetadata.Type, out var dic))
                    {
                        dic = new Dictionary<Signature, Dictionary<Signature, HashSet<UnicastMetadata>>>();
                        _unicastMetadatas[unicastMetadata.Type] = dic;
                    }

                    if (!dic.TryGetValue(unicastMetadata.Signature, out var dic2))
                    {
                        dic2 = new Dictionary<Signature, HashSet<UnicastMetadata>>();
                        dic[unicastMetadata.Signature] = dic2;
                    }

                    var signature = unicastMetadata.Certificate.GetSignature();

                    if (!dic2.TryGetValue(signature, out var hashset))
                    {
                        hashset = new HashSet<UnicastMetadata>();
                        dic2[signature] = hashset;
                    }

                    if (!hashset.Contains(unicastMetadata))
                    {
                        if (!unicastMetadata.VerifyCertificate()) return false;

                        hashset.Add(unicastMetadata);
                    }

                    return true;
                }
            }

            public bool SetMetadata(MulticastMetadata multicastMetadata)
            {
                lock (_lockObject)
                {
                    var now = DateTime.UtcNow;

                    if (multicastMetadata == null
                        || multicastMetadata.Type == null
                        || multicastMetadata.Tag == null
                            || multicastMetadata.Tag.Id == null || multicastMetadata.Tag.Id.Length == 0
                            || string.IsNullOrWhiteSpace(multicastMetadata.Tag.Name)
                        || (multicastMetadata.CreationTime - now).TotalMinutes > 30
                        || multicastMetadata.Certificate == null) return false;

                    if (!_multicastMetadatas.TryGetValue(multicastMetadata.Type, out var dic))
                    {
                        dic = new Dictionary<Tag, Dictionary<Signature, HashSet<MulticastMetadata>>>();
                        _multicastMetadatas[multicastMetadata.Type] = dic;
                    }

                    if (!dic.TryGetValue(multicastMetadata.Tag, out var dic2))
                    {
                        dic2 = new Dictionary<Signature, HashSet<MulticastMetadata>>();
                        dic[multicastMetadata.Tag] = dic2;
                    }

                    var signature = multicastMetadata.Certificate.GetSignature();

                    if (!dic2.TryGetValue(signature, out var hashset))
                    {
                        hashset = new HashSet<MulticastMetadata>();
                        dic2[signature] = hashset;
                    }

                    if (!hashset.Contains(multicastMetadata))
                    {
                        if (!multicastMetadata.VerifyCertificate()) return false;

                        hashset.Add(multicastMetadata);
                    }

                    return true;
                }
            }
        }
    }
}
