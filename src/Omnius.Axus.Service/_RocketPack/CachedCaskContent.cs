namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedSeedBox
{
    public IEnumerable<CachedSeed> ToCachedSeeds()
    {
        var results = new List<CachedSeed>();

        foreach (var seed in this.Value.Seeds)
        {
            var result = new CachedSeed(this.Signature, seed);
            results.Add(result);
        }

        return results;
    }
}
