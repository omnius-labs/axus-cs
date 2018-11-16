using Amoeba.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using Omnius.Utils;
using Omnius.Collections;
using Omnius.Base;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public sealed class ContentInfosManager : ISetOperators<Hash>, IEnumerable<ContentInfo>
        {
            private Dictionary<Metadata, ContentInfo> _messageContentInfos;
            private Dictionary<string, ContentInfo> _fileContentInfos;

            private HashMap _hashMap;

            public ContentInfosManager()
            {
                _messageContentInfos = new Dictionary<Metadata, ContentInfo>();
                _fileContentInfos = new Dictionary<string, ContentInfo>();

                _hashMap = new HashMap();
            }

            public void Add(ContentInfo info)
            {
                if (info.ShareInfo == null)
                {
                    _messageContentInfos.Add(info.Metadata, info);
                }
                else
                {
                    _fileContentInfos.Add(info.ShareInfo.Path, info);

                    _hashMap.Add(info.ShareInfo);
                }
            }

            #region Message

            public void RemoveMessageContentInfo(Metadata metadata)
            {
                _messageContentInfos.Remove(metadata);
            }

            public bool ContainsMessageContentInfo(Metadata metadata)
            {
                return _messageContentInfos.ContainsKey(metadata);
            }

            public IEnumerable<ContentInfo> GetMessageContentInfos()
            {
                return _messageContentInfos.Values.ToArray();
            }

            public ContentInfo GetMessageContentInfo(Metadata metadata)
            {
                ContentInfo contentInfo;
                if (!_messageContentInfos.TryGetValue(metadata, out contentInfo)) return null;

                return contentInfo;
            }

            #endregion

            #region Content

            public void RemoveFileContentInfo(string path)
            {
                if (_fileContentInfos.TryGetValue(path, out var ContentInfo))
                {
                    _fileContentInfos.Remove(path);

                    _hashMap.Remove(ContentInfo.ShareInfo);
                }
            }

            public bool ContainsFileContentInfo(string path)
            {
                return _fileContentInfos.ContainsKey(path);
            }

            public IEnumerable<ContentInfo> GetFileContentInfos()
            {
                return _fileContentInfos.Values.ToArray();
            }

            public ContentInfo GetFileContentInfo(string path)
            {
                ContentInfo contentInfo;
                if (!_fileContentInfos.TryGetValue(path, out contentInfo)) return null;

                return contentInfo;
            }

            #endregion

            #region Hash

            public bool Contains(Hash hash)
            {
                return _hashMap.Contains(hash);
            }

            public IEnumerable<Hash> IntersectFrom(IEnumerable<Hash> collection)
            {
                foreach (var hash in collection)
                {
                    if (_hashMap.Contains(hash))
                    {
                        yield return hash;
                    }
                }
            }

            public IEnumerable<Hash> ExceptFrom(IEnumerable<Hash> collection)
            {
                foreach (var hash in collection)
                {
                    if (!_hashMap.Contains(hash))
                    {
                        yield return hash;
                    }
                }
            }

            public ShareInfo GetShareInfo(Hash hash)
            {
                return _hashMap.Get(hash);
            }

            public IEnumerable<Hash> GetHashes()
            {
                return _hashMap.ToArray();
            }

            #endregion

            #region IEnumerable<ContentInfo>

            public IEnumerator<ContentInfo> GetEnumerator()
            {
                foreach (var info in CollectionUtils.Unite(_messageContentInfos.Values, _fileContentInfos.Values))
                {
                    yield return info;
                }
            }

            #endregion

            #region IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

            sealed class HashMap
            {
                private Dictionary<Hash, SmallList<ShareInfo>> _map;

                public HashMap()
                {
                    _map = new Dictionary<Hash, SmallList<ShareInfo>>();
                }

                public void Add(ShareInfo info)
                {
                    foreach (var hash in info.Hashes)
                    {
                        _map.GetOrAdd(hash, (_) => new SmallList<ShareInfo>()).Add(info);
                    }
                }

                public void Remove(ShareInfo info)
                {
                    foreach (var hash in info.Hashes)
                    {
                        if (_map.TryGetValue(hash, out var infos))
                        {
                            infos.Remove(info);

                            if (infos.Count == 0)
                            {
                                _map.Remove(hash);
                            }
                        }
                    }
                }

                public ShareInfo Get(Hash hash)
                {
                    if (_map.TryGetValue(hash, out var infos))
                    {
                        return infos[0];
                    }

                    return null;
                }

                public bool Contains(Hash hash)
                {
                    return _map.ContainsKey(hash);
                }

                public Hash[] ToArray()
                {
                    return _map.Keys.ToArray();
                }
            }
        }
    }

    sealed partial class ShareInfo
    {
        #region Hash to Index

        private Dictionary<Hash, int> _hashMap = null;

        public int GetIndex(Hash hash)
        {
            if (_hashMap == null)
            {
                _hashMap = new Dictionary<Hash, int>();

                for (int i = 0; i < this.Hashes.Count; i++)
                {
                    _hashMap[this.Hashes[i]] = i;
                }
            }

            {
                int result;
                if (!_hashMap.TryGetValue(hash, out result)) return -1;

                return result;
            }
        }

        #endregion
    }
}
