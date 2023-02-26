using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Models;

public record ProfileReport
{
    public ProfileReport(OmniSignature signature, DateTime createdTime, Profile profile)
    {
        this.Signature = signature;
        this.CreatedTime = createdTime;
        this.Profile = profile;
    }

    public OmniSignature Signature { get; }
    public DateTime CreatedTime { get; }
    public Profile Profile { get; }
}
