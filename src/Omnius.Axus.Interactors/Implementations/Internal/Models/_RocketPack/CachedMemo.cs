using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedMemo
{
    private OmniHash _selfHash = OmniHash.Empty;

    private OmniHash ComputeSelfHash()
    {
        var bytesPipe = new BytesPipe(BytesPool.Shared);
        this.Export(bytesPipe.Writer, BytesPool.Shared);
        var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bytesPipe.Reader.GetSequence()));
        return hash;
    }

    public OmniHash SelfHash
    {
        get
        {
            if (_selfHash == OmniHash.Empty)
            {
                _selfHash = this.ComputeSelfHash();
            }

            return _selfHash;
        }
    }

    public MemoReport ToReport()
    {
        var result = new MemoReport(this.Signature, this.SelfHash, this.Value);
        return result;
    }
}
