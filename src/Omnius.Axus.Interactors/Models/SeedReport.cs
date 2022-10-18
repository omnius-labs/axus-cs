using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Models;

public record SeedReport
{
    public SeedReport(OmniSignature signature, Seed seed)
    {
        this.Signature = signature;
        this.Seed = seed;
    }

    public OmniSignature Signature { get; }
    public Seed Seed { get; }
}
