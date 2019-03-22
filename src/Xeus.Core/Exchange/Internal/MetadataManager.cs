using System;
using System.Collections.Generic;
using System.Linq;
using Amoeba.Messages;
using Omnix.Base;
using Omnix.Base.Extensions;
using Omnix.Collections;
using Omnix.Cryptography;
using Xeus.Messages;

namespace Xeus.Core.Exchange.Internal
{
    internal sealed class MetadataManager
    {
        // Type, AuthorSignature
        private Dictionary<string, Dictionary<OmniSignature, BroadcastMetadata>> _broadcastMetadatas = new Dictionary<string, Dictionary<OmniSignature, BroadcastMetadata>>();
        // Type, Signature, AuthorSignature
        private Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastMetadata>>>> _unicastMetadatas = new Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastMetadata>>>>();
        // Type, Channel, AuthorSignature
        private Dictionary<string, Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastMetadata>>>> _multicastMetadatas = new Dictionary<string, Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastMetadata>>>>();

        // UpdateTime
        private Dictionary<string, DateTime> _broadcastTypes = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> _unicastTypes = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> _multicastTypes = new Dictionary<string, DateTime>();

        // Alive
        private VolatileHashSet<Channel> _aliveChannels = new VolatileHashSet<Channel>(new TimeSpan(0, 30, 0));

        private readonly object _lockObject = new object();

        public MetadataManager()
        {

        }

        public GetSignaturesEventHandler GetLockedSignaturesEvent { get; set; }

        private IEnumerable<OmniSignature> OnGetLockedSignaturesEvent()
        {
            return this.GetLockedSignaturesEvent?.Invoke(this) ?? Enumerable.Empty<OmniSignature>();
        }

        public void Refresh()
        {
            lock (_lockObject)
            {
                var lockedSignatures = new HashSet<OmniSignature>(this.OnGetLockedSignaturesEvent());
                var lockedChannels = new HashSet<Channel>(_aliveChannels);

                _aliveChannels.Update();

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
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

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
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _unicastMetadatas.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

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
                        var keys = dic.Keys.Where(n => !lockedChannels.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _multicastMetadatas.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

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

        public IEnumerable<OmniSignature> GetBroadcastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_broadcastMetadatas.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<OmniSignature> GetUnicastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_unicastMetadatas.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<Channel> GetMulticastChannels()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<Channel>();

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

        public IEnumerable<BroadcastMetadata> GetBroadcastMetadatas(OmniSignature signature)
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

        public BroadcastMetadata GetBroadcastMetadata(OmniSignature signature, string type)
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
                return _unicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<UnicastMetadata> GetUnicastMetadatas(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<UnicastMetadata>();

                foreach (var dic in _unicastMetadatas.Values)
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<UnicastMetadata> GetUnicastMetadatas(OmniSignature signature, string type)
        {
            lock (_lockObject)
            {
                _unicastTypes[type] = DateTime.UtcNow;

                var list = new List<UnicastMetadata>();

                if (_unicastMetadatas.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<MulticastMetadata> GetMulticastMetadatas()
        {
            lock (_lockObject)
            {
                return _multicastMetadatas.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Channel channel)
        {
            lock (_lockObject)
            {
                var list = new List<MulticastMetadata>();

                foreach (var dic in _multicastMetadatas.Values)
                {
                    if (dic.TryGetValue(channel, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Channel channel, string type)
        {
            lock (_lockObject)
            {
                _aliveChannels.Add(channel);
                _multicastTypes[type] = DateTime.UtcNow;

                var list = new List<MulticastMetadata>();

                if (_multicastMetadatas.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(channel, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
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
                    || (broadcastMetadata.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || broadcastMetadata.Certificate == null) return false;

                if (!_broadcastMetadatas.TryGetValue(broadcastMetadata.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, BroadcastMetadata>();
                    _broadcastMetadatas[broadcastMetadata.Type] = dic;
                }

                var signature = broadcastMetadata.Certificate.GetOmniSignature();

                if (!dic.TryGetValue(signature, out var tempMetadata)
                    || broadcastMetadata.CreationTime.ToDateTime() > tempMetadata.CreationTime.ToDateTime())
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
                        || unicastMetadata.Signature.Hash.Value.Length == 0
                        || string.IsNullOrWhiteSpace(unicastMetadata.Signature.Name)
                    || (unicastMetadata.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || unicastMetadata.Certificate == null) return false;

                if (!_unicastMetadatas.TryGetValue(unicastMetadata.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastMetadata>>>();
                    _unicastMetadatas[unicastMetadata.Type] = dic;
                }

                if (!dic.TryGetValue(unicastMetadata.Signature, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<UnicastMetadata>>();
                    dic[unicastMetadata.Signature] = dic2;
                }

                var signature = unicastMetadata.Certificate.GetOmniSignature();

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
                    || multicastMetadata.Channel == null
                        || multicastMetadata.Channel.Id.Length == 0
                        || string.IsNullOrWhiteSpace(multicastMetadata.Channel.Name)
                    || (multicastMetadata.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || multicastMetadata.Certificate == null) return false;

                if (!_multicastMetadatas.TryGetValue(multicastMetadata.Type, out var dic))
                {
                    dic = new Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastMetadata>>>();
                    _multicastMetadatas[multicastMetadata.Type] = dic;
                }

                if (!dic.TryGetValue(multicastMetadata.Channel, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<MulticastMetadata>>();
                    dic[multicastMetadata.Channel] = dic2;
                }

                var signature = multicastMetadata.Certificate.GetOmniSignature();

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
