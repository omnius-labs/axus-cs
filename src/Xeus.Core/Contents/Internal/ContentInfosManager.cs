using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omnix.Base.Extensions;
using Omnix.Base.Helpers;
using Omnix.Collections;
using Omnix.Cryptography;
using Xeus.Messages;

namespace Xeus.Core.Contents.Internal
{
    internal sealed class ContentInfosManager : ISetOperators<OmniHash>, IEnumerable<ContentInfo>
    {
        private Dictionary<Clue, ContentInfo> _messageContentInfos;
        private Dictionary<string, ContentInfo> _fileContentInfos;

        private HashMap _hashMap;

        public ContentInfosManager()
        {
            _messageContentInfos = new Dictionary<Clue, ContentInfo>();
            _fileContentInfos = new Dictionary<string, ContentInfo>();

            _hashMap = new HashMap();
        }

        public void Add(ContentInfo info)
        {
            if (info.SharedBlocksInfo == null)
            {
                _messageContentInfos.Add(info.Clue, info);
            }
            else
            {
                _fileContentInfos.Add(info.SharedBlocksInfo.Path, info);

                _hashMap.Add(info.SharedBlocksInfo);
            }
        }

        #region Message

        public void RemoveMessageContentInfo(Clue clue)
        {
            _messageContentInfos.Remove(clue);
        }

        public bool ContainsMessageContentInfo(Clue clue)
        {
            return _messageContentInfos.ContainsKey(clue);
        }

        public IEnumerable<ContentInfo> GetMessageContentInfos()
        {
            return _messageContentInfos.Values.ToArray();
        }

        public ContentInfo GetMessageContentInfo(Clue clue)
        {
            ContentInfo contentInfo;
            if (!_messageContentInfos.TryGetValue(clue, out contentInfo)) return null;

            return contentInfo;
        }

        #endregion

        #region Content

        public void RemoveFileContentInfo(string path)
        {
            if (_fileContentInfos.TryGetValue(path, out var contentInfo))
            {
                _fileContentInfos.Remove(path);

                _hashMap.Remove(contentInfo.SharedBlocksInfo);
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

        #region OmniHash

        public bool Contains(OmniHash hash)
        {
            return _hashMap.Contains(hash);
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            foreach (var hash in collection)
            {
                if (_hashMap.Contains(hash))
                {
                    yield return hash;
                }
            }
        }

        public IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection)
        {
            foreach (var hash in collection)
            {
                if (!_hashMap.Contains(hash))
                {
                    yield return hash;
                }
            }
        }

        public SharedBlocksInfo GetSharedBlocksInfo(OmniHash hash)
        {
            return _hashMap.Get(hash);
        }

        public IEnumerable<OmniHash> GetHashes()
        {
            return _hashMap.ToArray();
        }

        #endregion

        #region IEnumerable<ContentInfo>

        public IEnumerator<ContentInfo> GetEnumerator()
        {
            foreach (var info in CollectionHelper.Unite(_messageContentInfos.Values, _fileContentInfos.Values))
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
            private Dictionary<OmniHash, SmallList<SharedBlocksInfo>> _map;

            public HashMap()
            {
                _map = new Dictionary<OmniHash, SmallList<SharedBlocksInfo>>();
            }

            public void Add(SharedBlocksInfo info)
            {
                foreach (var hash in info.Hashes)
                {
                    _map.GetOrAdd(hash, (_) => new SmallList<SharedBlocksInfo>()).Add(info);
                }
            }

            public void Remove(SharedBlocksInfo info)
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

            public SharedBlocksInfo Get(OmniHash hash)
            {
                if (_map.TryGetValue(hash, out var infos))
                {
                    return infos[0];
                }

                return null;
            }

            public bool Contains(OmniHash hash)
            {
                return _map.ContainsKey(hash);
            }

            public OmniHash[] ToArray()
            {
                return _map.Keys.ToArray();
            }
        }
    }
}
