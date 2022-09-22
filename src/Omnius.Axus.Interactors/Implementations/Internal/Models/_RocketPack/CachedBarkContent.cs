namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedBarkContent
{
    public IEnumerable<CachedBarkMessage> ToMessages()
    {
        var results = new List<CachedBarkMessage>();

        foreach (var message in this.Value.Messages)
        {
            var result = new CachedBarkMessage(this.Signature, message);
            results.Add(result);
        }

        return results;
    }
}
