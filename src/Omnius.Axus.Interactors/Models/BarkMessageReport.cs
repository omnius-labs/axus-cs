using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Models;

public record BarkMessageReport
{
    public BarkMessageReport(OmniSignature signature, string tag, DateTime createdTime, string comment, OmniHash anchorHash, OmniHash selfHash)
    {
        this.Signature = signature;
        this.Tag = tag;
        this.CreatedTime = createdTime;
        this.Comment = comment;
        this.AnchorHash = anchorHash;
        this.SelfHash = selfHash;
    }

    public OmniSignature Signature { get; }
    public string Tag { get; }
    public DateTime CreatedTime { get; }
    public string Comment { get; }
    public OmniHash AnchorHash { get; private set; }
    public OmniHash SelfHash { get; private set; }
}
