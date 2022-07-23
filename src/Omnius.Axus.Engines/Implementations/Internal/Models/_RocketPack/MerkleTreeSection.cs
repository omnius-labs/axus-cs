using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal partial class MerkleTreeSection
{
    private Dictionary<OmniHash, long>? _indexMap = null;
    private readonly object _indexMapLockObject = new();

    private Dictionary<OmniHash, long> GetIndexMap()
    {
        if (_indexMap == null)
        {
            lock (_indexMapLockObject)
            {
                if (_indexMap == null)
                {
                    _indexMap = new Dictionary<OmniHash, long>();

                    for (int i = 0; i < this.Hashes.Count; i++)
                    {
                        _indexMap[this.Hashes[i]] = i;
                    }
                }
            }
        }

        return _indexMap;
    }

    public bool Contains(OmniHash targetHash)
    {
        return this.GetIndexMap().ContainsKey(targetHash);
    }

    public bool TryGetIndex(OmniHash targetHash, out long index)
    {
        return this.GetIndexMap().TryGetValue(targetHash, out index);
    }
}
