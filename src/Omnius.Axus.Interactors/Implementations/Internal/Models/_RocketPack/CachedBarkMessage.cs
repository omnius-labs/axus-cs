using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedBarkMessage
{
    public BarkMessageReport ToReport()
    {
        return new BarkMessageReport(
            this.Signature,
            this.Value.Tag,
            this.Value.CreatedTime.ToDateTime(),
            this.Value.Comment,
            this.Value.AnchorHash,
            this.SelfHash
        );
    }

    public OmniHash SelfHash
    {
        get
        {
            // TODO
            return OmniHash.Empty;
        }
    }
}
