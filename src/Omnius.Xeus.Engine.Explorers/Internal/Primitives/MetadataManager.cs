using System;
using System.Collections.Generic;
using System.Linq;
using Omnius.Core.Cryptography;
using Omnius.Core;
using Omnius.Core.Extensions;
using Xeus.Engine.Internal.Search;
using Xeus.Messages;

namespace Xeus.Engine.Internal.Search.Primitives
{
    public delegate IEnumerable<OmniSignature> GetSignaturesEventHandler();

    internal sealed class MetadataManager
    {
        // Type, AuthorSignature
        private readonly Dictionary<string, Dictionary<OmniSignature, BroadcastClue>> _broadcastClues = new Dictionary<string, Dictionary<OmniSignature, BroadcastClue>>();
        // Type, Signature, AuthorSignature
        private readonly Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>> _unicastClues = new Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>>();
        // Type, Signature, AuthorSignature
        private readonly Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<MulticastClue>>>> _multicastClues = new Dictionary<string, Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<MulticastClue>>>>();

        // UpdateTime
        private readonly Dictionary<string, DateTime> _broadcastTypes = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, DateTime> _unicastTypes = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, DateTime> _multicastTypes = new Dictionary<string, DateTime>();

        private readonly object _lockObject = new object();

        public MetadataManager()
        {

        }

        public GetSignaturesEventHandler GetLockedSignaturesEvent { private get; set; } = () => Enumerable.Empty<OmniSignature>();

        private IEnumerable<OmniSignature> OnGetLockedSignaturesEvent()
        {
            return this.GetLockedSignaturesEvent.Invoke();
        }

        public void Refresh()
        {
            lock (_lockObject)
            {
                var lockedSignatures = new HashSet<OmniSignature>(this.OnGetLockedSignaturesEvent());

                // Broadcast
                {
                    {
                        var removeTypes = new HashSet<string>(_broadcastTypes.OrderBy(n => n.Value).Select(n => n.Key).Take(_broadcastTypes.Count - 32));

                        foreach (string type in removeTypes)
                        {
                            _broadcastTypes.Remove(type);
                        }

                        foreach (string type in _broadcastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _broadcastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _broadcastClues.Values)
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

                        foreach (string type in _unicastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _unicastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _unicastClues.Values)
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _unicastClues.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 32))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var hashset in _unicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
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

                        foreach (string type in _multicastClues.Keys.ToArray())
                        {
                            if (!removeTypes.Contains(type)) continue;
                            _multicastClues.Remove(type);
                        }
                    }

                    foreach (var dic in _multicastClues.Values)
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 1024))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var dic in _multicastClues.Values.SelectMany(n => n.Values))
                    {
                        var keys = dic.Keys.Where(n => !lockedSignatures.Contains(n)).ToList();

                        foreach (var key in keys.Randomize().Take(keys.Count - 32))
                        {
                            dic.Remove(key);
                        }
                    }

                    foreach (var hashset in _multicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values)).ToArray())
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

                    count += _broadcastClues.Values.Sum(n => n.Count);
                    count += _unicastClues.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));
                    count += _multicastClues.Values.Sum(n => n.Values.Sum(m => m.Values.Sum(o => o.Count)));

                    return count;
                }
            }
        }

        public IEnumerable<OmniSignature> GetBroadcastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_broadcastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<OmniSignature> GetUnicastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_unicastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<OmniSignature> GetMulticastSignatures()
        {
            lock (_lockObject)
            {
                var hashset = new HashSet<OmniSignature>();

                hashset.UnionWith(_multicastClues.Values.SelectMany(n => n.Keys));

                return hashset;
            }
        }

        public IEnumerable<BroadcastClue> GetBroadcastClues()
        {
            lock (_lockObject)
            {
                return _broadcastClues.Values.SelectMany(n => n.Values).ToArray();
            }
        }

        public IEnumerable<BroadcastClue> GetBroadcastClues(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<BroadcastClue>();

                foreach (var dic in _broadcastClues.Values)
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

                if (_broadcastClues.TryGetValue(type, out var dic))
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
                return _unicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<UnicastClue> GetUnicastClues(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<UnicastClue>();

                foreach (var dic in _unicastClues.Values)
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

                if (_unicastClues.TryGetValue(type, out var dic))
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
                return _multicastClues.Values.SelectMany(n => n.Values.SelectMany(m => m.Values.SelectMany(o => o))).ToArray();
            }
        }

        public IEnumerable<MulticastClue> GetMulticastClues(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var list = new List<MulticastClue>();

                foreach (var dic in _multicastClues.Values)
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public IEnumerable<MulticastClue> GetMulticastClues(OmniSignature signature, string type)
        {
            lock (_lockObject)
            {
                _multicastTypes[type] = DateTime.UtcNow;

                var list = new List<MulticastClue>();

                if (_multicastClues.TryGetValue(type, out var dic))
                {
                    if (dic.TryGetValue(signature, out var dic2))
                    {
                        list.AddRange(dic2.Values.SelectMany(n => n));
                    }
                }

                return list;
            }
        }

        public bool SetMetadata(BroadcastClue broadcastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (broadcastClue == null
                    || broadcastClue.Type == null
                    || (broadcastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || broadcastClue.Certificate == null) return false;

                if (!_broadcastClues.TryGetValue(broadcastClue.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, BroadcastClue>();
                    _broadcastClues[broadcastClue.Type] = dic;
                }

                var signature = broadcastClue.Certificate.GetOmniSignature();

                if (!dic.TryGetValue(signature, out var tempMetadata)
                    || broadcastClue.CreationTime.ToDateTime() > tempMetadata.CreationTime.ToDateTime())
                {
                    if (!broadcastClue.VerifyCertificate()) return false;

                    dic[signature] = broadcastClue;
                }

                return true;
            }
        }

        public bool SetMetadata(UnicastClue unicastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (unicastClue == null
                    || unicastClue.Type == null
                    || unicastClue.Signature == null
                        || unicastClue.Signature.Hash.Value.Length == 0
                        || string.IsNullOrWhiteSpace(unicastClue.Signature.Name)
                    || (unicastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || unicastClue.Certificate == null) return false;

                if (!_unicastClues.TryGetValue(unicastClue.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<UnicastClue>>>();
                    _unicastClues[unicastClue.Type] = dic;
                }

                if (!dic.TryGetValue(unicastClue.Signature, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<UnicastClue>>();
                    dic[unicastClue.Signature] = dic2;
                }

                var signature = unicastClue.Certificate.GetOmniSignature();

                if (!dic2.TryGetValue(signature, out var hashset))
                {
                    hashset = new HashSet<UnicastClue>();
                    dic2[signature] = hashset;
                }

                if (!hashset.Contains(unicastClue))
                {
                    if (!unicastClue.VerifyCertificate()) return false;

                    hashset.Add(unicastClue);
                }

                return true;
            }
        }

        public bool SetMetadata(MulticastClue multicastClue)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (multicastClue == null
                    || multicastClue.Type == null
                    || multicastClue.Signature == null
                        || multicastClue.Signature.Hash.Value.Length == 0
                        || string.IsNullOrWhiteSpace(multicastClue.Signature.Name)
                    || (multicastClue.CreationTime.ToDateTime() - now).TotalMinutes > 30
                    || multicastClue.Certificate == null) return false;

                if (!_multicastClues.TryGetValue(multicastClue.Type, out var dic))
                {
                    dic = new Dictionary<OmniSignature, Dictionary<OmniSignature, HashSet<MulticastClue>>>();
                    _multicastClues[multicastClue.Type] = dic;
                }

                if (!dic.TryGetValue(multicastClue.Signature, out var dic2))
                {
                    dic2 = new Dictionary<OmniSignature, HashSet<MulticastClue>>();
                    dic[multicastClue.Signature] = dic2;
                }

                var signature = multicastClue.Certificate.GetOmniSignature();

                if (!dic2.TryGetValue(signature, out var hashset))
                {
                    hashset = new HashSet<MulticastClue>();
                    dic2[signature] = hashset;
                }

                if (!hashset.Contains(multicastClue))
                {
                    if (!multicastClue.VerifyCertificate()) return false;

                    hashset.Add(multicastClue);
                }

                return true;
            }
        }
    }
}
