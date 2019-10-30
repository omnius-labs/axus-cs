using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omnix.Cryptography;
using Omnix.Base.Helpers;

namespace Xeus.Core.Repositories.Internal
{
    internal sealed class ContentMemoryStorage : ISetOperators<OmniHash>, IEnumerable<ContentMetadata>
    {
        private readonly Dictionary<Clue, ContentMetadata> _messageMap;
        private readonly Dictionary<string, ContentMetadata> _fileMap;

        private readonly HashMap _hashMap;

        public ContentMemoryStorage()
        {
            _messageMap = new Dictionary<Clue, ContentMetadata>();
            _fileMap = new Dictionary<string, ContentMetadata>();

            _hashMap = new HashMap();
        }

        public void Add(ContentMetadata metadata)
        {
            if (metadata.SharedFileMetadata == null)
            {
                _messageMap.Add(metadata.Clue, metadata);
            }
            else
            {
                _fileMap.Add(metadata.SharedFileMetadata.Path, metadata);

                _hashMap.Add(metadata);
            }
        }

        #region Message

        public void RemoveMessageContentMetadata(Clue clue)
        {
            _messageMap.Remove(clue);
        }

        public bool ContainsMessageContentMetadata(Clue clue)
        {
            return _messageMap.ContainsKey(clue);
        }

        public IEnumerable<ContentMetadata> GetMessageContentMetadatas()
        {
            return _messageMap.Values.ToArray();
        }

        public ContentMetadata? GetMessageContentMetadata(Clue clue)
        {
            if (!_messageMap.TryGetValue(clue, out var metadata))
            {
                return null;
            }

            return metadata;
        }

        #endregion

        #region Content

        public void RemoveFileContentMetadata(string path)
        {
            if (_fileMap.TryGetValue(path, out var metadata))
            {
                _fileMap.Remove(path);

                if (metadata != null)
                {
                    _hashMap.Remove(metadata);
                }
            }
        }

        public bool ContainsFileContentMetadata(string path)
        {
            return _fileMap.ContainsKey(path);
        }

        public IEnumerable<ContentMetadata> GetFileContentMetadatas()
        {
            return _fileMap.Values.ToArray();
        }

        public ContentMetadata? GetFileContentMetadata(string path)
        {
            if (!_fileMap.TryGetValue(path, out var contentInfo))
            {
                return null;
            }

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

        public bool TryGetSharedFileInfo(OmniHash hash)
        {
            var metadata = _hashMap.Get(hash);
            if (metadata == null)
            {
                return false;
            }

            // TODO
            //metadata.SharedFileMetadata.

            return true;
        }

        public IEnumerable<OmniHash> GetHashes()
        {
            return _hashMap.ToArray();
        }

        #endregion

        #region IEnumerable<ContentMetadata>

        public IEnumerator<ContentMetadata> GetEnumerator()
        {
            foreach (var metadata in CollectionHelper.Unite(_messageMap.Values, _fileMap.Values))
            {
                yield return metadata;
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        private sealed class HashMap
        {
            private readonly HashSet<ContentMetadata> _hashSet = new HashSet<ContentMetadata>();

            private Dictionary<OmniHash, ContentMetadata>? _map = null;

            public HashMap()
            {
            }

            private void Update()
            {
                if (_map is null)
                {
                    _map = new Dictionary<OmniHash, ContentMetadata>();

                    foreach (var metadata in _hashSet)
                    {
                        foreach (var hash in metadata.MerkleTreeNodes[^1].Hashes)
                        {
                            _map[hash] = metadata;
                        }
                    }
                }
            }

            public void Add(ContentMetadata metadata)
            {
                _hashSet.Add(metadata);

                _map = null;
            }

            public void Remove(ContentMetadata metadata)
            {
                _hashSet.Remove(metadata);

                _map = null;
            }

            public ContentMetadata? Get(OmniHash hash)
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
