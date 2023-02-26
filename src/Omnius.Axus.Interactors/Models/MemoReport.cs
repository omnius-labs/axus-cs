using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Models;

public record MemoReport
{
    public MemoReport(OmniSignature signature, OmniHash selfHash, Memo memo)
    {
        this.Signature = signature;
        this.SelfHash = selfHash;
        this.Memo = memo;
    }

    public OmniSignature Signature { get; }
    public OmniHash SelfHash { get; }
    public Memo Memo { get; }
}
