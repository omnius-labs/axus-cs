using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omnix.Base.Extensions;
using Omnix.Base.Helpers;
using Omnix.Collections;
using Omnix.Cryptography;
using Xeus.Core.Contents.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Contents.Internal
{
    internal sealed class ContentMetadataStorage : ISetOperators<OmniHash>, IEnumerable<ContentMetadata>
    {
        private readonly Dictionary<XeusClue, ContentMetadata> _messageContentMetadatas;
        private readonly Dictionary<string, ContentMetadata> _fileContentMetadatas;

        private readonly HashMap _hashMap;

        public ContentMetadataStorage()
        {
            _messageContentMetadatas = new Dictionary<XeusClue, ContentMetadata>();
            _fileContentMetadatas = new Dictionary<string, ContentMetadata>();

            _hashMap = new HashMap();
        }

        public void Add(ContentMetadata info)
        {
            if (info.SharedBlocksMetadata == null)
            {
                _messageContentMetadatas.Add(info.Clue, info);
            }
            else
            {
                _fileContentMetadatas.Add(info.SharedBlocksMetadata.Path, info);

                _hashMap.Add(info.SharedBlocksMetadata);
            }
        }

        #region Message

        public void RemoveMessageContentMetadata(XeusClue clue)
        {
            _messageContentMetadatas.Remove(clue);
        }

        public bool ContainsMessageContentMetadata(XeusClue clue)
        {
            return _messageContentMetadatas.ContainsKey(clue);
        }

        public IEnumerable<ContentMetadata> GetMessageContentMetadatas()
        {
            return _messageContentMetadatas.Values.ToArray();
        }

        public ContentMetadata? GetMessageContentMetadata(XeusClue clue)
        {
            ContentMetadata contentInfo;
            if (!_messageContentMetadatas.TryGetValue(clue, out contentInfo)) return null;

            return contentInfo;
        }

        #endregion

        #region Content

        public void RemoveFileContentMetadata(string path)
        {
            if (_fileContentMetadatas.TryGetValue(path, out var contentInfo))
            {
                _fileContentMetadatas.Remove(path);

                if (contentInfo.SharedBlocksMetadata != null)
                {
                    _hashMap.Remove(contentInfo.SharedBlocksMetadata);
                }
            }
        }

        public bool ContainsFileContentMetadata(string path)
        {
            return _fileContentMetadatas.ContainsKey(path);
        }

        public IEnumerable<ContentMetadata> GetFileContentMetadatas()
        {
            return _fileContentMetadatas.Values.ToArray();
        }

        public ContentMetadata? GetFileContentMetadata(string path)
        {
            ContentMetadata contentInfo;
            if (!_fileContentMetadatas.TryGetValue(path, out contentInfo)) return null;

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

        public SharedBlocksMetadata? GetSharedBlocksInfo(OmniHash hash)
        {
            return _hashMap.Get(hash);
        }

        public IEnumerable<OmniHash> GetHashes()
        {
            return _hashMap.ToArray();
        }

        #endregion

        #region IEnumerable<ContentMetadata>

        public IEnumerator<ContentMetadata> GetEnumerator()
        {
            foreach (var info in CollectionHelper.Unite(_messageContentMetadatas.Values, _fileContentMetadatas.Values))
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
            private readonly HashSet<SharedBlocksMetadata> _hashSet = new HashSet<SharedBlocksMetadata>();

            private Dictionary<OmniHash, SharedBlocksMetadata>? _map = null;

            public HashMap()
            {
            }

            private void Update()
            {
                if (_map is null)
                {
                    _map = new Dictionary<OmniHash, SharedBlocksMetadata>();

                    foreach (var metadata in _hashSet)
                    {
                        foreach (var hash in metadata.Hashes)
                        {
                            _map[hash] = metadata;
                        }
                    }
                }
            }

            public void Add(SharedBlocksMetadata metadata)
            {
                _hashSet.Add(metadata);

                _map = null;
            }

            public void Remove(SharedBlocksMetadata metadata)
            {
                _hashSet.Remove(metadata);

                _map = null;
            }

            public SharedBlocksMetadata? Get(OmniHash hash)
            {
                this.Update();

                if (_map!.TryGetValue(hash, out var metadata))
                {
                    return metadata;
                }

                return null;
            }

            public bool Contains(OmniHash hash)
            {
                this.Update();

                return _map!.ContainsKey(hash);
            }

            public OmniHash[] ToArray()
            {
                this.Update();

                return _map!.Keys.ToArray();
            }
        }
    }
}
