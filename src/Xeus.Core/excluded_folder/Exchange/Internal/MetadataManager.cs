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
        private Dictionary<string, Dictionary<OmniSignature, BroadcastClue>> _BroadcastClues = new Dictionary<string, Dictionary<OmniSignature, BroadcastClue>>();
        // Type, Signature, AuthorSignature
        private Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>> _UnicastClues = new Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>>();
        // Type, Channel, AuthorSignature
        private Dictionary<string, Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastClue>>>> _MulticastClues = new Dictionary<string, Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastClue>>>>();

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

                        foreach (string type in _BroadcastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _BroadcastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _BroadcastClues.Values)
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

                        foreach (string type in _UnicastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _UnicastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _UnicastClues.Values)
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _UnicastClues.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 32))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var hashset in _UnicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
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

                        foreach (string type in _MulticastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _MulticastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _MulticastClues.Values)
                    {
                        var keys = dic.Keys.Where(n => !lockedChannels.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _MulticastClues.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 32))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var hashset in _MulticastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
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

                    count += _BroadcastClues.Values.Sum(n => n.Count);
                    count += _UnicastClues.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));
                    count += _MulticastClues.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));

                    return count;
                }
            }
        }

        public IEnumerable<OmniSignature> GetBroadcastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_BroadcastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<OmniSignature> GetUnicastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_UnicastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<Channel> GetMulticastChannels()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<Channel>();

                hashset.UnionWith(_MulticastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<BroadcastClue> GetBroadcastClues()
        {
            lock (_lockObject)
            {
                return _BroadcastClues.Values.SelectMany(n => n.Values).ToArray();
            }
        }

        public IEnumerable<BroadcastClue> GetBroadcastClues(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<BroadcastClue>();

                foreach (var dic in _BroadcastClues.Values)
                {
                    if (dic.TryGetValue(signature, out var BroadcastClue))
                    {
                        list.Add(BroadcastClue);
                    }
                }

                return list;
            }
        }

        public BroadcastClue GetBroadcastClue(OmniSignature signature, string type)
        {
            lock (_lockObject)
            {
                _broadcastTypes[type] = DateTime.UtcNow;

                if (_BroadcastClues.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(signature, out var BroadcastClue))
                    {
                        return BroadcastClue;
                    }
                }

                return null;
            }
        }

        public IEnumerable<UnicastClue> GetUnicastClues()
        {
            lock (_lockObject)
            {
                return _UnicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<UnicastClue> GetUnicastClues(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<UnicastClue>();

                foreach (var dic in _UnicastClues.Values)
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<UnicastClue> GetUnicastClues(OmniSignature signature, string type)
        {
            lock (_lockObject)
            {
                _unicastTypes[type] = DateTime.UtcNow;

                var list = new List<UnicastClue>();

                if (_UnicastClues.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<MulticastClue> GetMulticastClues()
        {
            lock (_lockObject)
            {
                return _MulticastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<MulticastClue> GetMulticastClues(Channel channel)
        {
            lock (_lockObject)
            {
                var list = new List<MulticastClue>();

                foreach (var dic in _MulticastClues.Values)
                {
                    if (dic.TryGetValue(channel, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<MulticastClue> GetMulticastClues(Channel channel, string type)
        {
            lock (_lockObject)
            {
                _aliveChannels.Add(channel);
                _multicastTypes[type] = DateTime.UtcNow;

                var list = new List<MulticastClue>();

                if (_MulticastClues.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(channel, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public bool SetMetadata(BroadcastClue BroadcastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (BroadcastClue == null
                    || BroadcastClue.Type == null
                    || (BroadcastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || BroadcastClue.Certificate == null) return false;

                if (!_BroadcastClues.TryGetValue(BroadcastClue.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, BroadcastClue>();
                    _BroadcastClues[BroadcastClue.Type] = dic;
                }

                var signature = BroadcastClue.Certificate.GetOmniSignature();

                if (!dic.TryGetValue(signature, out var tempMetadata)
                    || BroadcastClue.CreationTime.ToDateTime() > tempMetadata.CreationTime.ToDateTime())
                {
                    if (!BroadcastClue.VerifyCertificate()) return false;

                    dic[signature] = BroadcastClue;
                }

                return true;
            }
        }

        public bool SetMetadata(UnicastClue UnicastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (UnicastClue == null
                    || UnicastClue.Type == null
                    || UnicastClue.Signature == null
                        || UnicastClue.Signature.Hash.Value.Length == 0
                        || string.IsNullOrWhiteSpace(UnicastClue.Signature.Name)
                    || (UnicastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || UnicastClue.Certificate == null) return false;

                if (!_UnicastClues.TryGetValue(UnicastClue.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>();
                    _UnicastClues[UnicastClue.Type] = dic;
                }

                if (!dic.TryGetValue(UnicastClue.Signature, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<UnicastClue>>();
                    dic[UnicastClue.Signature] = dic2;
                }

                var signature = UnicastClue.Certificate.GetOmniSignature();

                if (!dic2.TryGetValue(signature, out var hashset))
                {
                    hashset = new HashSet<UnicastClue>();
                    dic2[signature] = hashset;
                }

                if (!hashset.Contains(UnicastClue))
                {
                    if (!UnicastClue.VerifyCertificate()) return false;

                    hashset.Add(UnicastClue);
                }

                return true;
            }
        }

        public bool SetMetadata(MulticastClue MulticastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (MulticastClue == null
                    || MulticastClue.Type == null
                    || MulticastClue.Channel == null
                        || MulticastClue.Channel.Id.Length == 0
                        || string.IsNullOrWhiteSpace(MulticastClue.Channel.Name)
                    || (MulticastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || MulticastClue.Certificate == null) return false;

                if (!_MulticastClues.TryGetValue(MulticastClue.Type, out var dic))
                {
                    dic = new Dictionary<Channel, Dictionary<OmniSignature, HashSet<MulticastClue>>>();
                    _MulticastClues[MulticastClue.Type] = dic;
                }

                if (!dic.TryGetValue(MulticastClue.Channel, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<MulticastClue>>();
                    dic[MulticastClue.Channel] = dic2;
                }

                var signature = MulticastClue.Certificate.GetOmniSignature();

                if (!dic2.TryGetValue(signature, out var hashset))
                {
                    hashset = new HashSet<MulticastClue>();
                    dic2[signature] = hashset;
                }

                if (!hashset.Contains(MulticastClue))
                {
                    if (!MulticastClue.VerifyCertificate()) return false;

                    hashset.Add(MulticastClue);
                }

                return true;
            }
        }
    }
}
