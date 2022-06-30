using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Models;

public record SubscribedProfileReport
{
    public SubscribedProfileReport(DateTime createdTime, OmniSignature signature)
    {
        this.CreatedTime = createdTime;
        this.Signature = signature;
    }

    public DateTime CreatedTime { get; }

    public OmniSignature Signature { get; }
}