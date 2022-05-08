using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors.Models;

public record PublishedProfileReport
{
    public PublishedProfileReport(DateTime createdTime, OmniSignature signature)
    {
        this.CreatedTime = createdTime;
        this.Signature = signature;
    }

    public DateTime CreatedTime { get; }

    public OmniSignature Signature { get; }
}
