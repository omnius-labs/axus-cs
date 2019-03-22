using System;
using System.Collections.Generic;
using System.Text;
using Omnix.Cryptography;

namespace Xeus.Core.Contents.Internal
{
    partial class SharedBlocksInfo
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
                int result;
                if (!_hashMap.TryGetValue(hash, out result)) return -1;

                return result;
            }
        }

        #endregion
    }
}
