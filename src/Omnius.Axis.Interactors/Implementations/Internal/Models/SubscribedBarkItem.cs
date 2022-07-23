using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Internal.Models;

internal record SubscribedBarkItem
{
    public SubscribedBarkItem(string tag, OmniSignature signature, string comment, OmniHash selfHash, OmniHash? replyHash, DateTime messageCreatedTime, DateTime packageCreatedTime)
    {
        this.Tag = tag;
        this.Signature = signature;
        this.Comment = comment;
        this.SelfHash = selfHash;
        this.ReplyHash = replyHash;
        this.MessageCreatedTime = messageCreatedTime;
        this.PackageCreatedTime = packageCreatedTime;
    }

    public string Tag { get; }

    public OmniSignature Signature { get; }

    public string Comment { get; }

    public OmniHash SelfHash { get; }

    public OmniHash? ReplyHash { get; }

    public DateTime MessageCreatedTime { get; }

    public DateTime PackageCreatedTime { get; }
}
