using System.Collections.Generic;
using Omnix.Algorithms.Cryptography;

namespace Xeus.Core.Repositories.Internal
{
    internal partial class SharedFileMetadata
    {
        #region OmniHash to Index

        private Dictionary<OmniHash, int> _hashMap = null;

        public int GetIndex(OmniHash hash)
        {
            if (_hashMap == null)
            {
                _hashMap = new Dictionary<OmniHash, int>();

                for (int i = 0; i < this.Hashes.Count; i++)
                {
                    _hashMap[this.Hashes[i]] = i;
                }
            }

            {
                if (!_hashMap.TryGetValue(hash, out int result))
                {
                    return -1;
                }

                return result;
            }
        }

        #endregion
    }
}
