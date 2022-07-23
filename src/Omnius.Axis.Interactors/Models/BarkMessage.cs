using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Models;

public record BarkMessage
{
    public BarkMessage(OmniSignature signature, DateTime createdTime, string comment, BarkReply? reply)
    {
        this.Signature = signature;
        this.CreatedTime = createdTime;
        this.Comment = comment;
        this.Reply = reply;
    }

    public OmniSignature Signature { get; }
    public DateTime CreatedTime { get; }
    public string Comment { get; }
    public BarkReply? Reply { get; }
}
